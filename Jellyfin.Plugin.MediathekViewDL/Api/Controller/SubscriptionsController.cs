using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Extensions;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Controller;

/// <summary>
/// The Controller to manage the Subscriptions.
/// </summary>
[ApiController]
[Route("MediathekViewDL/[controller]")]
[Authorize(Policy = Policies.RequiresElevation)]
public class SubscriptionsController : ControllerBase
{
    private readonly IConfigurationProvider _configurationProvider;
    private readonly IDownloadHistoryRepository _downloadHistoryRepository;
    private readonly ISubscriptionProcessor _subscriptionProcessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionsController"/> class.
    /// </summary>
    /// <param name="configurationProvider">The subscription service.</param>
    /// <param name="downloadHistoryRepository">The download history repository.</param>
    /// <param name="subscriptionProcessor">The subscription processor.</param>
    public SubscriptionsController(IConfigurationProvider configurationProvider, IDownloadHistoryRepository downloadHistoryRepository, ISubscriptionProcessor subscriptionProcessor)
    {
        _configurationProvider = configurationProvider;
        _downloadHistoryRepository = downloadHistoryRepository;
        _subscriptionProcessor = subscriptionProcessor;
    }

    /// <summary>
    /// Gets all subscriptions.
    /// </summary>
    /// <returns>All subscriptions.</returns>
    [HttpGet]
    public ActionResult<List<Subscription>> GetSubscriptions()
    {
        var subscriptions = _configurationProvider.Configuration.Subscriptions;
        return Ok(subscriptions);
    }

    /// <summary>
    /// Gets a subscription by its ID.
    /// </summary>
    /// <param name="id">The ID of the Subscription.</param>
    /// <returns>The Subscription.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Subscription), 200)]
    [ProducesResponseType(404)]
    public ActionResult<Subscription> GetSubscription(Guid id)
    {
        var subscription = _configurationProvider.Configuration.Subscriptions.FirstOrDefault(s => s.Id == id);
        if (subscription == null)
        {
            return NotFound("Subscription not found");
        }

        return Ok(subscription);
    }

    /// <summary>
    /// Creates a new subscription.
    /// </summary>
    /// <param name="subscription">The Subscription to create.</param>
    /// <returns>The created Subscription.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Subscription), 201)]
    [ProducesResponseType(400)]
    public ActionResult<Subscription> CreateSubscription([FromBody] Subscription subscription)
    {
        if (subscription == null)
        {
            return BadRequest("Subscription cannot be null");
        }

        subscription.Id = Guid.NewGuid();
        _configurationProvider.Configuration.Subscriptions.Add(subscription);
        if (_configurationProvider.TrySave())
        {
            return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, subscription);
        }

        _configurationProvider.Configuration.Subscriptions.Remove(subscription);
        return BadRequest("Failed to create subscription");
    }

    /// <summary>
    /// Updates an existing subscription.
    /// </summary>
    /// <param name="id">The ID of the Subscription to update.</param>
    /// <param name="subscription">The New Subscription.</param>
    /// <returns>The Updated Subscription.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Subscription), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    [ProducesResponseType(404)]
    public ActionResult<Subscription> UpdateSubscription(Guid id, [FromBody] Subscription subscription)
    {
        if (subscription == null)
        {
            return BadRequest("Subscription cannot be null");
        }

        var existingSubscriptionIndex = _configurationProvider.Configuration.Subscriptions.FindIndex(s => s.Id == id);
        if (existingSubscriptionIndex == -1)
        {
            return NotFound("Subscription not found");
        }

        var oldSubscription = _configurationProvider.Configuration.Subscriptions[existingSubscriptionIndex];
        subscription.Id = id;
        _configurationProvider.Configuration.Subscriptions[existingSubscriptionIndex] = subscription;

        if (_configurationProvider.TrySave())
        {
            return Ok(subscription);
        }

        _configurationProvider.Configuration.Subscriptions[existingSubscriptionIndex] = oldSubscription;
        return BadRequest("Failed to update subscription");
    }

    /// <summary>
    /// Deletes a subscription by its ID.
    /// </summary>
    /// <param name="id">The ID of the Subscription to delete.</param>
    /// <returns>Nothing.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Subscription), StatusCodes.Status204NoContent)]
    [ProducesResponseType(404)]
    public ActionResult DeleteSubscription(Guid id)
    {
        var existingSubscriptionIndex = _configurationProvider.Configuration.Subscriptions.FindIndex(s => s.Id == id);
        if (existingSubscriptionIndex == -1)
        {
            return NotFound("Subscription not found");
        }

        var oldSubscription = _configurationProvider.Configuration.Subscriptions[existingSubscriptionIndex];
        _configurationProvider.Configuration.Subscriptions.RemoveAt(existingSubscriptionIndex);

        if (_configurationProvider.TrySave())
        {
            return NoContent();
        }

        _configurationProvider.Configuration.Subscriptions.Insert(existingSubscriptionIndex, oldSubscription);
        return BadRequest("Failed to delete subscription");
    }

    /// <summary>
    /// Resets the download history of a subscription.
    /// </summary>
    /// <param name="id">The ID of the Subscription to Reset the History.</param>
    /// <returns>Noting.</returns>
    [HttpPost("{id}/ResetHistory")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ResetHistory(Guid id)
    {
        var subscription = _configurationProvider.Configuration.Subscriptions.FirstOrDefault(s => id == s.Id);

        if (subscription == null)
        {
            return NotFound("Subscription not found");
        }

        var oldTimestamp = subscription.LastDownloadedTimestamp;
        await _downloadHistoryRepository.RemoveBySubscriptionIdAsync(id).ConfigureAwait(false);
        subscription.LastDownloadedTimestamp = null;
        if (_configurationProvider.TrySave())
        {
            return Ok();
        }

        subscription.LastDownloadedTimestamp = oldTimestamp;
        return BadRequest("Failed to reset subscription history");
    }

    /// <summary>
    /// Processes a subscription. This will download all new items since the last download and update the last downloaded timestamp.
    /// </summary>
    /// <param name="id">The ID of the Subscription.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The number of new Items.</returns>
    [HttpPost("{id}/Process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> ProcessSubscription(Guid id, CancellationToken cancellationToken)
    {
        var subscription = _configurationProvider.Configuration.Subscriptions.FirstOrDefault(s => id == s.Id);

        if (subscription == null)
        {
            return NotFound("Subscription not found");
        }

        var count = await _subscriptionProcessor.ProcessSubscriptionAsync(subscription, cancellationToken).ConfigureAwait(false);
        _configurationProvider.TrySave();

        return Ok(count);
    }

    /// <summary>
    /// Sets the active state of a subscription.
    /// </summary>
    /// <param name="id">The ID of the Subscription.</param>
    /// <param name="active">Whether the subscription should be active.</param>
    /// <returns>The updated active state.</returns>
    [HttpPost("{id}/Active")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<bool> SetActiveState(Guid id, [FromQuery] bool active)
    {
        var subscription = _configurationProvider.Configuration.Subscriptions.FirstOrDefault(s => s.Id == id);

        if (subscription == null)
        {
            return NotFound("Subscription not found");
        }

        var oldActiveState = subscription.IsEnabled;
        subscription.IsEnabled = active;
        if (_configurationProvider.TrySave())
        {
            return Ok(subscription.IsEnabled);
        }

        subscription.IsEnabled = oldActiveState;
        return BadRequest("Failed to update subscription state");
    }

    /// <summary>
    /// Tests a subscription. This will return all items that would be downloaded if the subscription would be processed, but it won't update the last downloaded timestamp.
    /// </summary>
    /// <param name="subscription">The Subscription to Test.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>All items that would be downloaded if the subscription would be processed.</returns>
    [HttpPost("Test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ResultItemDto>>> TestSubscription([FromBody] Subscription subscription, CancellationToken cancellationToken)
    {
        var resultItemsEnumerable = _subscriptionProcessor.TestSubscriptionAsync(subscription, cancellationToken);
        List<ResultItemDto> result = new List<ResultItemDto>();
        await foreach (var item in resultItemsEnumerable.ConfigureAwait(false))
        {
            result.Add(item);
        }

        return Ok(result);
    }
}
