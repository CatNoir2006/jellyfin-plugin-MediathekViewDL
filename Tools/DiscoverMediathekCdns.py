import requests
import urllib.parse
from urllib.parse import urlparse

def get_mediathek_urls(search_queries, page_size=150):
    """
    Fetches media URLs from the MediathekViewWeb API based on search queries.

    Args:
        search_queries (list): A list of strings, each being a search term (e.g., channel names).
        page_size (int): The number of results to request per API call.

    Returns:
        tuple: A tuple containing two lists:
               - all_tlds (list): A list of top-level domains from all found URLs.
               - all_subdomains (list): A list of subdomains from all found URLs.
    """
    API_URL = "https://mediathekviewweb.de/api/query"
    all_urls = []

    for query_text in search_queries:
        payload = {
            "queries": [
                {
                    "fields": ["channel", "title", "topic"], # Search in channel, title and topic
                    "query": query_text
                }
            ],
            "sortBy": "timestamp",
            "sortOrder": "desc",
            "future": False,
            "offset": 0,
            "size": page_size
        }

        try:
            response = requests.post(API_URL, json=payload, headers={"Content-Type": "application/json"})
            response.raise_for_status()  # Raise an exception for HTTP errors
            data = response.json()

            if data and data.get("result") and data["result"].get("results"):
                for item in data["result"]["results"]:
                    # Collect all relevant URL types
                    if item.get("url_video_hd"):
                        all_urls.append(item["url_video_hd"])
                    if item.get("url_video"):
                        all_urls.append(item["url_video"])
                    if item.get("url_video_low"):
                        all_urls.append(item["url_video_low"])
                    if item.get("url_subtitle"):
                        all_urls.append(item["url_subtitle"])
            elif data and data.get("err"):
                print(f"API Error for query '{query_text}': {data['err']}")

        except requests.exceptions.RequestException as e:
            print(f"Request failed for query '{query_text}': {e}")
        except ValueError as e:
            print(f"Failed to parse JSON response for query '{query_text}': {e}")
        except Exception as e:
            print(f"An unexpected error occurred for query '{query_text}': {e}")

    all_full_domains = []
    all_main_domains = []

    for url_str in all_urls:
        try:
            parsed_url = urlparse(url_str)
            hostname = parsed_url.hostname
            if hostname:
                all_full_domains.append(hostname)

                parts = hostname.split('.')
                if len(parts) >= 2:
                    # Simplified approach: consider the last two parts as the main domain.
                    # This might not be perfectly accurate for all complex TLDs (e.g., co.uk)
                    main_domain = ".".join(parts[-2:])
                    all_main_domains.append(main_domain)
                elif len(parts) == 1:
                    all_main_domains.append(hostname) # e.g. "localhost"

        except Exception as e:
            print(f"Error parsing URL '{url_str}': {e}")

    # Remove duplicates and sort
    all_full_domains = sorted(list(set(all_full_domains)))
    all_main_domains = sorted(list(set(all_main_domains)))

    return all_full_domains, all_main_domains

if __name__ == "__main__":
    # Example Usage:
    search_terms = ["ARD", "ZDF", "WDR", "SWR", "BR", "NDR", "RBB", "MDR", "HR", "SR", "arte", "3sat", "KiKA", "Phoenix", "tagesschau", "heute", "ZDFinfo", "ZDFneo", "One", "ARDalpha", "ZDFkultur", "Märchen"]

    # Set a higher page size for more results per query
    results_per_page = 50

    print(f"Fetching URLs for search terms: {search_terms} with {results_per_page} results per page...")
    full_domains, main_domains = get_mediathek_urls(search_terms, results_per_page)

    print("\n--- Unique Full Domains (including subdomains) ---")
    for domain in full_domains:
        print(domain)

    print("\n--- Unique Main Domains (e.g., ardemediathek.de) ---")
    for domain in main_domains:
        print(domain)

    print("\n--- Unique Main Domains (e.g., ardemediathek.de) as Array---")
    print("[")
    print(",\n".join([f'"{domain}"' for domain in main_domains]))
    print("]")
