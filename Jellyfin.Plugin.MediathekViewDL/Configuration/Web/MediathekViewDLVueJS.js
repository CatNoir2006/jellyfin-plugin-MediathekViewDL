(function(){
          const style = document.createElement('style');
          style.textContent = ".search-form[data-v-0f1b9dfa]{width:100%;max-width:none;margin-bottom:20px}.search-grid[data-v-0f1b9dfa]{grid-template-columns:repeat(auto-fill,minmax(250px,1fr));gap:15px;width:100%;display:grid}.field[data-v-0f1b9dfa]{flex-direction:column;gap:5px;display:flex}.field label[data-v-0f1b9dfa]{color:#a1a1aa;font-size:.9rem}.field-input[data-v-0f1b9dfa]{color:#fff;box-sizing:border-box;background:#27272a;border:1px solid #3f3f46;border-radius:4px;width:100%;padding:10px}.field-input[data-v-0f1b9dfa]:focus{border-color:#7c3aed;outline:none}.form-actions[data-v-0f1b9dfa]{justify-content:flex-start;align-items:center;gap:15px;margin-top:20px;display:flex}.results-list[data-v-0f1b9dfa]{margin-top:20px}.result-item[data-v-0f1b9dfa]{border-bottom:1px solid #333;justify-content:space-between;align-items:center;padding:12px;display:flex}.result-item[data-v-0f1b9dfa]:last-child{border-bottom:none}.result-title[data-v-0f1b9dfa]{margin-bottom:2px;font-weight:700}.result-meta[data-v-0f1b9dfa]{color:#a1a1aa;align-items:center;gap:8px;font-size:.8rem;display:flex}.closed_caption[data-v-0f1b9dfa]{font-size:1.1rem}.result-actions[data-v-0f1b9dfa]{gap:10px;display:flex}.btn-icon[data-v-0f1b9dfa]{color:#fff;cursor:pointer;background:0 0;border:none;border-radius:4px;padding:5px;font-size:1.4rem}.btn-icon[data-v-0f1b9dfa]:hover{background:#3f3f46}.btn-primary[data-v-0f1b9dfa]{color:#fff;cursor:pointer;background:#7c3aed;border:none;border-radius:4px;height:42px;padding:10px 30px;font-weight:600}.btn-primary[data-v-0f1b9dfa]:disabled{opacity:.5;cursor:not-allowed}.btn-secondary[data-v-0f1b9dfa]{color:#fff;cursor:pointer;background:#3f3f46;border:none;border-radius:4px;height:42px;padding:10px 20px;font-weight:600}.btn-icon-only[data-v-0f1b9dfa]{padding:10px;font-size:1.2rem}.no-results[data-v-0f1b9dfa]{text-align:center;color:#a1a1aa;width:100%;padding:20px}.header-row[data-v-733f9dde]{justify-content:space-between;align-items:center;margin-bottom:20px;display:flex}.subscriptions-list[data-v-733f9dde]{gap:10px;display:grid}.subscription-item[data-v-733f9dde]{background:#27272a;border:1px solid #3f3f46;border-radius:8px;justify-content:space-between;align-items:center;padding:15px;display:flex}.subscription-item.disabled[data-v-733f9dde]{opacity:.6;border-style:dashed}.sub-name[data-v-733f9dde]{align-items:center;gap:10px;font-size:1.1rem;font-weight:700;display:flex}.sub-meta[data-v-733f9dde]{color:#a1a1aa;margin-top:4px;font-size:.85rem}.badge[data-v-733f9dde]{color:#e4e4e7;background:#3f3f46;border-radius:4px;padding:2px 8px;font-size:.7rem;font-weight:400}.sub-actions[data-v-733f9dde]{gap:15px;display:flex}.btn-icon[data-v-733f9dde]{cursor:pointer;filter:grayscale();color:#fff;background:0 0;border:none;border-radius:4px;padding:5px;font-size:1.4rem}.btn-icon[data-v-733f9dde]:hover{filter:none;background:#3f3f46}.btn-delete[data-v-733f9dde]:hover{color:#ef4444}.btn-primary[data-v-733f9dde]{color:#fff;cursor:pointer;background:#7c3aed;border:none;border-radius:4px;padding:10px 20px;font-weight:600}.btn-secondary[data-v-733f9dde]{color:#fff;cursor:pointer;background:#3f3f46;border:none;border-radius:4px;margin-top:10px;padding:8px 16px}.state-msg[data-v-733f9dde]{text-align:center;color:#a1a1aa;padding:40px}.error-container[data-v-733f9dde]{text-align:center;color:#ef4444;background:#ef44441a;border:1px solid #ef4444;border-radius:8px;padding:30px}.error-msg[data-v-733f9dde]{margin-bottom:10px;font-weight:700}.no-data[data-v-733f9dde]{text-align:center;color:#a1a1aa;background:#27272a;border:1px dashed #3f3f46;border-radius:8px;padding:40px}.spinner[data-v-733f9dde]{border:3px solid #ffffff1a;border-top-color:#7c3aed;border-radius:50%;width:24px;height:24px;margin:0 auto 10px;animation:1s linear infinite spin-733f9dde}@keyframes spin-733f9dde{0%{transform:rotate(0)}to{transform:rotate(360deg)}}.editor-overlay[data-v-a6c8dd1b]{z-index:9999;background:#000c;justify-content:center;align-items:center;width:100%;height:100%;padding:20px;display:flex;position:fixed;top:0;left:0}.editor-modal[data-v-a6c8dd1b]{background:#18181b;border:1px solid #3f3f46;border-radius:8px;flex-direction:column;width:100%;max-width:800px;height:80vh;min-height:500px;display:flex;overflow:hidden;box-shadow:0 20px 25px -5px #00000080}.editor-header[data-v-a6c8dd1b]{border-bottom:1px solid #3f3f46;justify-content:space-between;align-items:center;padding:20px;display:flex}.editor-tabs[data-v-a6c8dd1b]{background:#27272a;border-bottom:1px solid #3f3f46;display:flex;overflow-x:auto}.editor-content[data-v-a6c8dd1b]{flex:1;padding:20px;overflow-y:auto}.editor-footer[data-v-a6c8dd1b]{border-top:1px solid #3f3f46;justify-content:flex-end;gap:15px;padding:20px;display:flex}.tab-btn[data-v-a6c8dd1b]{color:#a1a1aa;cursor:pointer;white-space:nowrap;background:0 0;border:none;padding:12px 20px}.tab-btn.active[data-v-a6c8dd1b]{color:#7c3aed;background:#18181b;border-bottom:2px solid #7c3aed}.field[data-v-a6c8dd1b]{margin-bottom:15px}.field label[data-v-a6c8dd1b]{color:#a1a1aa;margin-bottom:5px;font-size:.9rem;display:block}.field-input[data-v-a6c8dd1b]{color:#fff;box-sizing:border-box;background:#27272a;border:1px solid #3f3f46;border-radius:4px;width:100%;padding:10px}.field-desc[data-v-a6c8dd1b]{color:#71717a;margin-top:4px;font-size:.75rem;line-height:1.4}.checkbox-field[data-v-a6c8dd1b]{margin-bottom:15px}.checkbox-field label[data-v-a6c8dd1b]{cursor:pointer;align-items:center;gap:10px;font-weight:600;display:flex}.grid-2[data-v-a6c8dd1b]{grid-template-columns:1fr 1fr;gap:20px;display:grid}.query-row[data-v-a6c8dd1b]{background:#27272a;border:1px solid #3f3f46;border-radius:8px;margin-bottom:15px;padding:15px}.query-fields[data-v-a6c8dd1b]{gap:8px;margin-bottom:10px;display:flex}.field-tag[data-v-a6c8dd1b]{color:#a1a1aa;cursor:pointer;background:#3f3f46;border:none;border-radius:12px;padding:4px 10px;font-size:.75rem}.field-tag.active[data-v-a6c8dd1b]{color:#fff;background:#7c3aed}.query-input-row[data-v-a6c8dd1b]{align-items:center;gap:10px;display:flex}.input-with-btn[data-v-a6c8dd1b]{gap:10px;display:flex}.sub-options[data-v-a6c8dd1b]{border-left:2px solid #3f3f46;margin-top:10px;margin-bottom:10px;margin-left:25px;padding-left:15px}.btn-small[data-v-a6c8dd1b]{color:#fff;cursor:pointer;background:#27272a;border:1px solid #3f3f46;border-radius:4px;padding:5px 10px;font-size:.75rem}.btn-danger[data-v-a6c8dd1b]{background:#ef4444;border-color:#ef4444}.plugin-config[data-v-b10468a8]{color:#e4e4e7;box-sizing:border-box;width:100%;margin:0 auto;padding:1rem}.config-header[data-v-b10468a8]{margin-bottom:2rem}.tab-row[data-v-b10468a8]{border-bottom:1px solid #333;gap:10px;margin-bottom:20px;padding-bottom:10px;display:flex}.tab-btn[data-v-b10468a8]{color:#a1a1aa;cursor:pointer;background:0 0;border:none;padding:10px;font-weight:600}.tab-btn.active[data-v-b10468a8]{color:#7c3aed;border-bottom:2px solid #7c3aed}.modal-overlay[data-v-b10468a8]{z-index:10000;background:#000000f2;justify-content:center;align-items:center;width:100%;height:100%;padding:20px;display:flex;position:fixed;top:0;left:0}.modal-card[data-v-b10468a8]{background:#18181b;border:1px solid #3f3f46;border-radius:8px;flex-direction:column;width:100%;max-width:800px;max-height:80vh;display:flex;overflow:hidden}.modal-header[data-v-b10468a8]{border-bottom:1px solid #3f3f46;justify-content:space-between;align-items:center;padding:20px;display:flex}.modal-content[data-v-b10468a8]{flex:1;padding:20px;overflow-y:auto}.modal-footer[data-v-b10468a8]{border-top:1px solid #3f3f46;justify-content:flex-end;padding:20px;display:flex}.test-item[data-v-b10468a8]{border-bottom:1px solid #333;padding:12px}.test-item-title[data-v-b10468a8]{margin-bottom:2px;font-weight:700}.test-item-meta[data-v-b10468a8]{color:#a1a1aa;font-size:.8rem}.state-msg[data-v-b10468a8],.no-data[data-v-b10468a8]{text-align:center;color:#a1a1aa;padding:40px}.spinner[data-v-b10468a8]{border:3px solid #ffffff1a;border-top-color:#7c3aed;border-radius:50%;width:24px;height:24px;margin:0 auto 10px;animation:1s linear infinite spin-b10468a8}@keyframes spin-b10468a8{0%{transform:rotate(0)}to{transform:rotate(360deg)}}.btn-icon[data-v-b10468a8]{color:#fff;cursor:pointer;background:0 0;border:none;font-size:1.2rem}.btn-primary[data-v-b10468a8]{color:#fff;cursor:pointer;background:#7c3aed;border:none;border-radius:4px;padding:10px 20px;font-weight:600}\n/*$vite$:1*/";
          document.head.appendChild(style);
        })();
//#region node_modules/@vue/shared/dist/shared.esm-bundler.js
/* @__NO_SIDE_EFFECTS__ */
function e(e) {
	let t = /* @__PURE__ */ Object.create(null);
	for (let n of e.split(",")) t[n] = 1;
	return (e) => e in t;
}
var t = {}, n = [], r = () => {}, i = () => !1, a = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && (e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), o = (e) => e.startsWith("onUpdate:"), s = Object.assign, c = (e, t) => {
	let n = e.indexOf(t);
	n > -1 && e.splice(n, 1);
}, l = Object.prototype.hasOwnProperty, u = (e, t) => l.call(e, t), d = Array.isArray, f = (e) => x(e) === "[object Map]", p = (e) => x(e) === "[object Set]", m = (e) => x(e) === "[object Date]", h = (e) => typeof e == "function", g = (e) => typeof e == "string", _ = (e) => typeof e == "symbol", v = (e) => typeof e == "object" && !!e, y = (e) => (v(e) || h(e)) && h(e.then) && h(e.catch), b = Object.prototype.toString, x = (e) => b.call(e), S = (e) => x(e).slice(8, -1), C = (e) => x(e) === "[object Object]", w = (e) => g(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, ee = /* @__PURE__ */ e(",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"), te = (e) => {
	let t = /* @__PURE__ */ Object.create(null);
	return ((n) => t[n] || (t[n] = e(n)));
}, ne = /-\w/g, T = te((e) => e.replace(ne, (e) => e.slice(1).toUpperCase())), re = /\B([A-Z])/g, E = te((e) => e.replace(re, "-$1").toLowerCase()), ie = te((e) => e.charAt(0).toUpperCase() + e.slice(1)), ae = te((e) => e ? `on${ie(e)}` : ""), D = (e, t) => !Object.is(e, t), oe = (e, ...t) => {
	for (let n = 0; n < e.length; n++) e[n](...t);
}, O = (e, t, n, r = !1) => {
	Object.defineProperty(e, t, {
		configurable: !0,
		enumerable: !1,
		writable: r,
		value: n
	});
}, se = (e) => {
	let t = parseFloat(e);
	return isNaN(t) ? e : t;
}, ce, le = () => ce ||= typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {};
function ue(e) {
	if (d(e)) {
		let t = {};
		for (let n = 0; n < e.length; n++) {
			let r = e[n], i = g(r) ? me(r) : ue(r);
			if (i) for (let e in i) t[e] = i[e];
		}
		return t;
	} else if (g(e) || v(e)) return e;
}
var de = /;(?![^(]*\))/g, fe = /:([^]+)/, pe = /\/\*[^]*?\*\//g;
function me(e) {
	let t = {};
	return e.replace(pe, "").split(de).forEach((e) => {
		if (e) {
			let n = e.split(fe);
			n.length > 1 && (t[n[0].trim()] = n[1].trim());
		}
	}), t;
}
function k(e) {
	let t = "";
	if (g(e)) t = e;
	else if (d(e)) for (let n = 0; n < e.length; n++) {
		let r = k(e[n]);
		r && (t += r + " ");
	}
	else if (v(e)) for (let n in e) e[n] && (t += n + " ");
	return t.trim();
}
var he = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", ge = /* @__PURE__ */ e(he);
he + "";
function _e(e) {
	return !!e || e === "";
}
function ve(e, t) {
	if (e.length !== t.length) return !1;
	let n = !0;
	for (let r = 0; n && r < e.length; r++) n = ye(e[r], t[r]);
	return n;
}
function ye(e, t) {
	if (e === t) return !0;
	let n = m(e), r = m(t);
	if (n || r) return n && r ? e.getTime() === t.getTime() : !1;
	if (n = _(e), r = _(t), n || r) return e === t;
	if (n = d(e), r = d(t), n || r) return n && r ? ve(e, t) : !1;
	if (n = v(e), r = v(t), n || r) {
		if (!n || !r || Object.keys(e).length !== Object.keys(t).length) return !1;
		for (let n in e) {
			let r = e.hasOwnProperty(n), i = t.hasOwnProperty(n);
			if (r && !i || !r && i || !ye(e[n], t[n])) return !1;
		}
	}
	return String(e) === String(t);
}
function be(e, t) {
	return e.findIndex((e) => ye(e, t));
}
var xe = (e) => !!(e && e.__v_isRef === !0), A = (e) => g(e) ? e : e == null ? "" : d(e) || v(e) && (e.toString === b || !h(e.toString)) ? xe(e) ? A(e.value) : JSON.stringify(e, Se, 2) : String(e), Se = (e, t) => xe(t) ? Se(e, t.value) : f(t) ? { [`Map(${t.size})`]: [...t.entries()].reduce((e, [t, n], r) => (e[Ce(t, r) + " =>"] = n, e), {}) } : p(t) ? { [`Set(${t.size})`]: [...t.values()].map((e) => Ce(e)) } : _(t) ? Ce(t) : v(t) && !d(t) && !C(t) ? String(t) : t, Ce = (e, t = "") => _(e) ? `Symbol(${e.description ?? t})` : e, j, we = class {
	constructor(e = !1) {
		this.detached = e, this._active = !0, this._on = 0, this.effects = [], this.cleanups = [], this._isPaused = !1, this._warnOnRun = !0, this.__v_skip = !0, !e && j && (j.active ? (this.parent = j, this.index = (j.scopes ||= []).push(this) - 1) : (this._active = !1, this._warnOnRun = !1));
	}
	get active() {
		return this._active;
	}
	pause() {
		if (this._active) {
			this._isPaused = !0;
			let e, t;
			if (this.scopes) for (e = 0, t = this.scopes.length; e < t; e++) this.scopes[e].pause();
			for (e = 0, t = this.effects.length; e < t; e++) this.effects[e].pause();
		}
	}
	resume() {
		if (this._active && this._isPaused) {
			this._isPaused = !1;
			let e, t;
			if (this.scopes) for (e = 0, t = this.scopes.length; e < t; e++) this.scopes[e].resume();
			for (e = 0, t = this.effects.length; e < t; e++) this.effects[e].resume();
		}
	}
	run(e) {
		if (this._active) {
			let t = j;
			try {
				return j = this, e();
			} finally {
				j = t;
			}
		}
	}
	on() {
		++this._on === 1 && (this.prevScope = j, j = this);
	}
	off() {
		if (this._on > 0 && --this._on === 0) {
			if (j === this) j = this.prevScope;
			else {
				let e = j;
				for (; e;) {
					if (e.prevScope === this) {
						e.prevScope = this.prevScope;
						break;
					}
					e = e.prevScope;
				}
			}
			this.prevScope = void 0;
		}
	}
	stop(e) {
		if (this._active) {
			this._active = !1;
			let t, n;
			for (t = 0, n = this.effects.length; t < n; t++) this.effects[t].stop();
			for (this.effects.length = 0, t = 0, n = this.cleanups.length; t < n; t++) this.cleanups[t]();
			if (this.cleanups.length = 0, this.scopes) {
				for (t = 0, n = this.scopes.length; t < n; t++) this.scopes[t].stop(!0);
				this.scopes.length = 0;
			}
			if (!this.detached && this.parent && !e) {
				let e = this.parent.scopes.pop();
				e && e !== this && (this.parent.scopes[this.index] = e, e.index = this.index);
			}
			this.parent = void 0;
		}
	}
};
function Te() {
	return j;
}
var M, Ee = /* @__PURE__ */ new WeakSet(), De = class {
	constructor(e) {
		this.fn = e, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, j && (j.active ? j.effects.push(this) : this.flags &= -2);
	}
	pause() {
		this.flags |= 64;
	}
	resume() {
		this.flags & 64 && (this.flags &= -65, Ee.has(this) && (Ee.delete(this), this.trigger()));
	}
	notify() {
		this.flags & 2 && !(this.flags & 32) || this.flags & 8 || je(this);
	}
	run() {
		if (!(this.flags & 1)) return this.fn();
		this.flags |= 2, Ue(this), Pe(this);
		let e = M, t = N;
		M = this, N = !0;
		try {
			return this.fn();
		} finally {
			Fe(this), M = e, N = t, this.flags &= -3;
		}
	}
	stop() {
		if (this.flags & 1) {
			for (let e = this.deps; e; e = e.nextDep) Re(e);
			this.deps = this.depsTail = void 0, Ue(this), this.onStop && this.onStop(), this.flags &= -2;
		}
	}
	trigger() {
		this.flags & 64 ? Ee.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
	}
	runIfDirty() {
		Ie(this) && this.run();
	}
	get dirty() {
		return Ie(this);
	}
}, Oe = 0, ke, Ae;
function je(e, t = !1) {
	if (e.flags |= 8, t) {
		e.next = Ae, Ae = e;
		return;
	}
	e.next = ke, ke = e;
}
function Me() {
	Oe++;
}
function Ne() {
	if (--Oe > 0) return;
	if (Ae) {
		let e = Ae;
		for (Ae = void 0; e;) {
			let t = e.next;
			e.next = void 0, e.flags &= -9, e = t;
		}
	}
	let e;
	for (; ke;) {
		let t = ke;
		for (ke = void 0; t;) {
			let n = t.next;
			if (t.next = void 0, t.flags &= -9, t.flags & 1) try {
				t.trigger();
			} catch (t) {
				e ||= t;
			}
			t = n;
		}
	}
	if (e) throw e;
}
function Pe(e) {
	for (let t = e.deps; t; t = t.nextDep) t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function Fe(e) {
	let t, n = e.depsTail, r = n;
	for (; r;) {
		let e = r.prevDep;
		r.version === -1 ? (r === n && (n = e), Re(r), ze(r)) : t = r, r.dep.activeLink = r.prevActiveLink, r.prevActiveLink = void 0, r = e;
	}
	e.deps = t, e.depsTail = n;
}
function Ie(e) {
	for (let t = e.deps; t; t = t.nextDep) if (t.dep.version !== t.version || t.dep.computed && (Le(t.dep.computed) || t.dep.version !== t.version)) return !0;
	return !!e._dirty;
}
function Le(e) {
	if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === We) || (e.globalVersion = We, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Ie(e)))) return;
	e.flags |= 2;
	let t = e.dep, n = M, r = N;
	M = e, N = !0;
	try {
		Pe(e);
		let n = e.fn(e._value);
		(t.version === 0 || D(n, e._value)) && (e.flags |= 128, e._value = n, t.version++);
	} catch (e) {
		throw t.version++, e;
	} finally {
		M = n, N = r, Fe(e), e.flags &= -3;
	}
}
function Re(e, t = !1) {
	let { dep: n, prevSub: r, nextSub: i } = e;
	if (r && (r.nextSub = i, e.prevSub = void 0), i && (i.prevSub = r, e.nextSub = void 0), n.subs === e && (n.subs = r, !r && n.computed)) {
		n.computed.flags &= -5;
		for (let e = n.computed.deps; e; e = e.nextDep) Re(e, !0);
	}
	!t && !--n.sc && n.map && n.map.delete(n.key);
}
function ze(e) {
	let { prevDep: t, nextDep: n } = e;
	t && (t.nextDep = n, e.prevDep = void 0), n && (n.prevDep = t, e.nextDep = void 0);
}
var N = !0, Be = [];
function Ve() {
	Be.push(N), N = !1;
}
function He() {
	let e = Be.pop();
	N = e === void 0 ? !0 : e;
}
function Ue(e) {
	let { cleanup: t } = e;
	if (e.cleanup = void 0, t) {
		let e = M;
		M = void 0;
		try {
			t();
		} finally {
			M = e;
		}
	}
}
var We = 0, Ge = class {
	constructor(e, t) {
		this.sub = e, this.dep = t, this.version = t.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
	}
}, Ke = class {
	constructor(e) {
		this.computed = e, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
	}
	track(e) {
		if (!M || !N || M === this.computed) return;
		let t = this.activeLink;
		if (t === void 0 || t.sub !== M) t = this.activeLink = new Ge(M, this), M.deps ? (t.prevDep = M.depsTail, M.depsTail.nextDep = t, M.depsTail = t) : M.deps = M.depsTail = t, qe(t);
		else if (t.version === -1 && (t.version = this.version, t.nextDep)) {
			let e = t.nextDep;
			e.prevDep = t.prevDep, t.prevDep && (t.prevDep.nextDep = e), t.prevDep = M.depsTail, t.nextDep = void 0, M.depsTail.nextDep = t, M.depsTail = t, M.deps === t && (M.deps = e);
		}
		return t;
	}
	trigger(e) {
		this.version++, We++, this.notify(e);
	}
	notify(e) {
		Me();
		try {
			for (let e = this.subs; e; e = e.prevSub) e.sub.notify() && e.sub.dep.notify();
		} finally {
			Ne();
		}
	}
};
function qe(e) {
	if (e.dep.sc++, e.sub.flags & 4) {
		let t = e.dep.computed;
		if (t && !e.dep.subs) {
			t.flags |= 20;
			for (let e = t.deps; e; e = e.nextDep) qe(e);
		}
		let n = e.dep.subs;
		n !== e && (e.prevSub = n, n && (n.nextSub = e)), e.dep.subs = e;
	}
}
var Je = /* @__PURE__ */ new WeakMap(), Ye = /* @__PURE__ */ Symbol(""), Xe = /* @__PURE__ */ Symbol(""), Ze = /* @__PURE__ */ Symbol("");
function P(e, t, n) {
	if (N && M) {
		let t = Je.get(e);
		t || Je.set(e, t = /* @__PURE__ */ new Map());
		let r = t.get(n);
		r || (t.set(n, r = new Ke()), r.map = t, r.key = n), r.track();
	}
}
function Qe(e, t, n, r, i, a) {
	let o = Je.get(e);
	if (!o) {
		We++;
		return;
	}
	let s = (e) => {
		e && e.trigger();
	};
	if (Me(), t === "clear") o.forEach(s);
	else {
		let i = d(e), a = i && w(n);
		if (i && n === "length") {
			let e = Number(r);
			o.forEach((t, n) => {
				(n === "length" || n === Ze || !_(n) && n >= e) && s(t);
			});
		} else switch ((n !== void 0 || o.has(void 0)) && s(o.get(n)), a && s(o.get(Ze)), t) {
			case "add":
				i ? a && s(o.get("length")) : (s(o.get(Ye)), f(e) && s(o.get(Xe)));
				break;
			case "delete":
				i || (s(o.get(Ye)), f(e) && s(o.get(Xe)));
				break;
			case "set":
				f(e) && s(o.get(Ye));
				break;
		}
	}
	Ne();
}
function $e(e) {
	let t = /* @__PURE__ */ I(e);
	return t === e ? t : (P(t, "iterate", Ze), /* @__PURE__ */ F(e) ? t : t.map(Vt));
}
function et(e) {
	return P(e = /* @__PURE__ */ I(e), "iterate", Ze), e;
}
function tt(e, t) {
	return /* @__PURE__ */ Rt(e) ? Ht(/* @__PURE__ */ Lt(e) ? Vt(t) : t) : Vt(t);
}
var nt = {
	__proto__: null,
	[Symbol.iterator]() {
		return rt(this, Symbol.iterator, (e) => tt(this, e));
	},
	concat(...e) {
		return $e(this).concat(...e.map((e) => d(e) ? $e(e) : e));
	},
	entries() {
		return rt(this, "entries", (e) => (e[1] = tt(this, e[1]), e));
	},
	every(e, t) {
		return at(this, "every", e, t, void 0, arguments);
	},
	filter(e, t) {
		return at(this, "filter", e, t, (e) => e.map((e) => tt(this, e)), arguments);
	},
	find(e, t) {
		return at(this, "find", e, t, (e) => tt(this, e), arguments);
	},
	findIndex(e, t) {
		return at(this, "findIndex", e, t, void 0, arguments);
	},
	findLast(e, t) {
		return at(this, "findLast", e, t, (e) => tt(this, e), arguments);
	},
	findLastIndex(e, t) {
		return at(this, "findLastIndex", e, t, void 0, arguments);
	},
	forEach(e, t) {
		return at(this, "forEach", e, t, void 0, arguments);
	},
	includes(...e) {
		return st(this, "includes", e);
	},
	indexOf(...e) {
		return st(this, "indexOf", e);
	},
	join(e) {
		return $e(this).join(e);
	},
	lastIndexOf(...e) {
		return st(this, "lastIndexOf", e);
	},
	map(e, t) {
		return at(this, "map", e, t, void 0, arguments);
	},
	pop() {
		return ct(this, "pop");
	},
	push(...e) {
		return ct(this, "push", e);
	},
	reduce(e, ...t) {
		return ot(this, "reduce", e, t);
	},
	reduceRight(e, ...t) {
		return ot(this, "reduceRight", e, t);
	},
	shift() {
		return ct(this, "shift");
	},
	some(e, t) {
		return at(this, "some", e, t, void 0, arguments);
	},
	splice(...e) {
		return ct(this, "splice", e);
	},
	toReversed() {
		return $e(this).toReversed();
	},
	toSorted(e) {
		return $e(this).toSorted(e);
	},
	toSpliced(...e) {
		return $e(this).toSpliced(...e);
	},
	unshift(...e) {
		return ct(this, "unshift", e);
	},
	values() {
		return rt(this, "values", (e) => tt(this, e));
	}
};
function rt(e, t, n) {
	let r = et(e), i = r[t]();
	return r !== e && !/* @__PURE__ */ F(e) && (i._next = i.next, i.next = () => {
		let e = i._next();
		return e.done || (e.value = n(e.value)), e;
	}), i;
}
var it = Array.prototype;
function at(e, t, n, r, i, a) {
	let o = et(e), s = o !== e && !/* @__PURE__ */ F(e), c = o[t];
	if (c !== it[t]) {
		let t = c.apply(e, a);
		return s ? Vt(t) : t;
	}
	let l = n;
	o !== e && (s ? l = function(t, r) {
		return n.call(this, tt(e, t), r, e);
	} : n.length > 2 && (l = function(t, r) {
		return n.call(this, t, r, e);
	}));
	let u = c.call(o, l, r);
	return s && i ? i(u) : u;
}
function ot(e, t, n, r) {
	let i = et(e), a = i !== e && !/* @__PURE__ */ F(e), o = n, s = !1;
	i !== e && (a ? (s = r.length === 0, o = function(t, r, i) {
		return s && (s = !1, t = tt(e, t)), n.call(this, t, tt(e, r), i, e);
	}) : n.length > 3 && (o = function(t, r, i) {
		return n.call(this, t, r, i, e);
	}));
	let c = i[t](o, ...r);
	return s ? tt(e, c) : c;
}
function st(e, t, n) {
	let r = /* @__PURE__ */ I(e);
	P(r, "iterate", Ze);
	let i = r[t](...n);
	return (i === -1 || i === !1) && /* @__PURE__ */ zt(n[0]) ? (n[0] = /* @__PURE__ */ I(n[0]), r[t](...n)) : i;
}
function ct(e, t, n = []) {
	Ve(), Me();
	let r = (/* @__PURE__ */ I(e))[t].apply(e, n);
	return Ne(), He(), r;
}
var lt = /* @__PURE__ */ e("__proto__,__v_isRef,__isVue"), ut = new Set(/* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(_));
function dt(e) {
	_(e) || (e = String(e));
	let t = /* @__PURE__ */ I(this);
	return P(t, "has", e), t.hasOwnProperty(e);
}
var ft = class {
	constructor(e = !1, t = !1) {
		this._isReadonly = e, this._isShallow = t;
	}
	get(e, t, n) {
		if (t === "__v_skip") return e.__v_skip;
		let r = this._isReadonly, i = this._isShallow;
		if (t === "__v_isReactive") return !r;
		if (t === "__v_isReadonly") return r;
		if (t === "__v_isShallow") return i;
		if (t === "__v_raw") return n === (r ? i ? At : kt : i ? Ot : Dt).get(e) || Object.getPrototypeOf(e) === Object.getPrototypeOf(n) ? e : void 0;
		let a = d(e);
		if (!r) {
			let e;
			if (a && (e = nt[t])) return e;
			if (t === "hasOwnProperty") return dt;
		}
		let o = Reflect.get(e, t, /* @__PURE__ */ L(e) ? e : n);
		if ((_(t) ? ut.has(t) : lt(t)) || (r || P(e, "get", t), i)) return o;
		if (/* @__PURE__ */ L(o)) {
			let e = a && w(t) ? o : o.value;
			return r && v(e) ? /* @__PURE__ */ Ft(e) : e;
		}
		return v(o) ? r ? /* @__PURE__ */ Ft(o) : /* @__PURE__ */ Nt(o) : o;
	}
}, pt = class extends ft {
	constructor(e = !1) {
		super(!1, e);
	}
	set(e, t, n, r) {
		let i = e[t], a = d(e) && w(t);
		if (!this._isShallow) {
			let e = /* @__PURE__ */ Rt(i);
			if (!/* @__PURE__ */ F(n) && !/* @__PURE__ */ Rt(n) && (i = /* @__PURE__ */ I(i), n = /* @__PURE__ */ I(n)), !a && /* @__PURE__ */ L(i) && !/* @__PURE__ */ L(n)) return e || (i.value = n), !0;
		}
		let o = a ? Number(t) < e.length : u(e, t), s = Reflect.set(e, t, n, /* @__PURE__ */ L(e) ? e : r);
		return e === /* @__PURE__ */ I(r) && (o ? D(n, i) && Qe(e, "set", t, n, i) : Qe(e, "add", t, n)), s;
	}
	deleteProperty(e, t) {
		let n = u(e, t), r = e[t], i = Reflect.deleteProperty(e, t);
		return i && n && Qe(e, "delete", t, void 0, r), i;
	}
	has(e, t) {
		let n = Reflect.has(e, t);
		return (!_(t) || !ut.has(t)) && P(e, "has", t), n;
	}
	ownKeys(e) {
		return P(e, "iterate", d(e) ? "length" : Ye), Reflect.ownKeys(e);
	}
}, mt = class extends ft {
	constructor(e = !1) {
		super(!0, e);
	}
	set(e, t) {
		return !0;
	}
	deleteProperty(e, t) {
		return !0;
	}
}, ht = /* @__PURE__ */ new pt(), gt = /* @__PURE__ */ new mt(), _t = /* @__PURE__ */ new pt(!0), vt = (e) => e, yt = (e) => Reflect.getPrototypeOf(e);
function bt(e, t, n) {
	return function(...r) {
		let i = this.__v_raw, a = /* @__PURE__ */ I(i), o = f(a), c = e === "entries" || e === Symbol.iterator && o, l = e === "keys" && o, u = i[e](...r), d = n ? vt : t ? Ht : Vt;
		return !t && P(a, "iterate", l ? Xe : Ye), s(Object.create(u), { next() {
			let { value: e, done: t } = u.next();
			return t ? {
				value: e,
				done: t
			} : {
				value: c ? [d(e[0]), d(e[1])] : d(e),
				done: t
			};
		} });
	};
}
function xt(e) {
	return function(...t) {
		return e === "delete" ? !1 : e === "clear" ? void 0 : this;
	};
}
function St(e, t) {
	let n = {
		get(n) {
			let r = this.__v_raw, i = /* @__PURE__ */ I(r), a = /* @__PURE__ */ I(n);
			e || (D(n, a) && P(i, "get", n), P(i, "get", a));
			let { has: o } = yt(i), s = t ? vt : e ? Ht : Vt;
			if (o.call(i, n)) return s(r.get(n));
			if (o.call(i, a)) return s(r.get(a));
			r !== i && r.get(n);
		},
		get size() {
			let t = this.__v_raw;
			return !e && P(/* @__PURE__ */ I(t), "iterate", Ye), t.size;
		},
		has(t) {
			let n = this.__v_raw, r = /* @__PURE__ */ I(n), i = /* @__PURE__ */ I(t);
			return e || (D(t, i) && P(r, "has", t), P(r, "has", i)), t === i ? n.has(t) : n.has(t) || n.has(i);
		},
		forEach(n, r) {
			let i = this, a = i.__v_raw, o = /* @__PURE__ */ I(a), s = t ? vt : e ? Ht : Vt;
			return !e && P(o, "iterate", Ye), a.forEach((e, t) => n.call(r, s(e), s(t), i));
		}
	};
	return s(n, e ? {
		add: xt("add"),
		set: xt("set"),
		delete: xt("delete"),
		clear: xt("clear")
	} : {
		add(e) {
			let n = /* @__PURE__ */ I(this), r = yt(n), i = /* @__PURE__ */ I(e), a = !t && !/* @__PURE__ */ F(e) && !/* @__PURE__ */ Rt(e) ? i : e;
			return r.has.call(n, a) || D(e, a) && r.has.call(n, e) || D(i, a) && r.has.call(n, i) || (n.add(a), Qe(n, "add", a, a)), this;
		},
		set(e, n) {
			!t && !/* @__PURE__ */ F(n) && !/* @__PURE__ */ Rt(n) && (n = /* @__PURE__ */ I(n));
			let r = /* @__PURE__ */ I(this), { has: i, get: a } = yt(r), o = i.call(r, e);
			o ||= (e = /* @__PURE__ */ I(e), i.call(r, e));
			let s = a.call(r, e);
			return r.set(e, n), o ? D(n, s) && Qe(r, "set", e, n, s) : Qe(r, "add", e, n), this;
		},
		delete(e) {
			let t = /* @__PURE__ */ I(this), { has: n, get: r } = yt(t), i = n.call(t, e);
			i ||= (e = /* @__PURE__ */ I(e), n.call(t, e));
			let a = r ? r.call(t, e) : void 0, o = t.delete(e);
			return i && Qe(t, "delete", e, void 0, a), o;
		},
		clear() {
			let e = /* @__PURE__ */ I(this), t = e.size !== 0, n = e.clear();
			return t && Qe(e, "clear", void 0, void 0, void 0), n;
		}
	}), [
		"keys",
		"values",
		"entries",
		Symbol.iterator
	].forEach((r) => {
		n[r] = bt(r, e, t);
	}), n;
}
function Ct(e, t) {
	let n = St(e, t);
	return (t, r, i) => r === "__v_isReactive" ? !e : r === "__v_isReadonly" ? e : r === "__v_raw" ? t : Reflect.get(u(n, r) && r in t ? n : t, r, i);
}
var wt = { get: /* @__PURE__ */ Ct(!1, !1) }, Tt = { get: /* @__PURE__ */ Ct(!1, !0) }, Et = { get: /* @__PURE__ */ Ct(!0, !1) }, Dt = /* @__PURE__ */ new WeakMap(), Ot = /* @__PURE__ */ new WeakMap(), kt = /* @__PURE__ */ new WeakMap(), At = /* @__PURE__ */ new WeakMap();
function jt(e) {
	switch (e) {
		case "Object":
		case "Array": return 1;
		case "Map":
		case "Set":
		case "WeakMap":
		case "WeakSet": return 2;
		default: return 0;
	}
}
function Mt(e) {
	return e.__v_skip || !Object.isExtensible(e) ? 0 : jt(S(e));
}
/* @__NO_SIDE_EFFECTS__ */
function Nt(e) {
	return /* @__PURE__ */ Rt(e) ? e : It(e, !1, ht, wt, Dt);
}
/* @__NO_SIDE_EFFECTS__ */
function Pt(e) {
	return It(e, !1, _t, Tt, Ot);
}
/* @__NO_SIDE_EFFECTS__ */
function Ft(e) {
	return It(e, !0, gt, Et, kt);
}
function It(e, t, n, r, i) {
	if (!v(e) || e.__v_raw && !(t && e.__v_isReactive)) return e;
	let a = Mt(e);
	if (a === 0) return e;
	let o = i.get(e);
	if (o) return o;
	let s = new Proxy(e, a === 2 ? r : n);
	return i.set(e, s), s;
}
/* @__NO_SIDE_EFFECTS__ */
function Lt(e) {
	return /* @__PURE__ */ Rt(e) ? /* @__PURE__ */ Lt(e.__v_raw) : !!(e && e.__v_isReactive);
}
/* @__NO_SIDE_EFFECTS__ */
function Rt(e) {
	return !!(e && e.__v_isReadonly);
}
/* @__NO_SIDE_EFFECTS__ */
function F(e) {
	return !!(e && e.__v_isShallow);
}
/* @__NO_SIDE_EFFECTS__ */
function zt(e) {
	return e ? !!e.__v_raw : !1;
}
/* @__NO_SIDE_EFFECTS__ */
function I(e) {
	let t = e && e.__v_raw;
	return t ? /* @__PURE__ */ I(t) : e;
}
function Bt(e) {
	return !u(e, "__v_skip") && Object.isExtensible(e) && O(e, "__v_skip", !0), e;
}
var Vt = (e) => v(e) ? /* @__PURE__ */ Nt(e) : e, Ht = (e) => v(e) ? /* @__PURE__ */ Ft(e) : e;
/* @__NO_SIDE_EFFECTS__ */
function L(e) {
	return e ? e.__v_isRef === !0 : !1;
}
/* @__NO_SIDE_EFFECTS__ */
function R(e) {
	return Ut(e, !1);
}
function Ut(e, t) {
	return /* @__PURE__ */ L(e) ? e : new Wt(e, t);
}
var Wt = class {
	constructor(e, t) {
		this.dep = new Ke(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = t ? e : /* @__PURE__ */ I(e), this._value = t ? e : Vt(e), this.__v_isShallow = t;
	}
	get value() {
		return this.dep.track(), this._value;
	}
	set value(e) {
		let t = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ F(e) || /* @__PURE__ */ Rt(e);
		e = n ? e : /* @__PURE__ */ I(e), D(e, t) && (this._rawValue = e, this._value = n ? e : Vt(e), this.dep.trigger());
	}
};
function Gt(e) {
	return /* @__PURE__ */ L(e) ? e.value : e;
}
var Kt = {
	get: (e, t, n) => t === "__v_raw" ? e : Gt(Reflect.get(e, t, n)),
	set: (e, t, n, r) => {
		let i = e[t];
		return /* @__PURE__ */ L(i) && !/* @__PURE__ */ L(n) ? (i.value = n, !0) : Reflect.set(e, t, n, r);
	}
};
function qt(e) {
	return /* @__PURE__ */ Lt(e) ? e : new Proxy(e, Kt);
}
var Jt = class {
	constructor(e, t, n) {
		this.fn = e, this.setter = t, this._value = void 0, this.dep = new Ke(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = We - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !t, this.isSSR = n;
	}
	notify() {
		if (this.flags |= 16, !(this.flags & 8) && M !== this) return je(this, !0), !0;
	}
	get value() {
		let e = this.dep.track();
		return Le(this), e && (e.version = this.dep.version), this._value;
	}
	set value(e) {
		this.setter && this.setter(e);
	}
};
/* @__NO_SIDE_EFFECTS__ */
function Yt(e, t, n = !1) {
	let r, i;
	return h(e) ? r = e : (r = e.get, i = e.set), new Jt(r, i, n);
}
var Xt = {}, Zt = /* @__PURE__ */ new WeakMap(), Qt = void 0;
function $t(e, t = !1, n = Qt) {
	if (n) {
		let t = Zt.get(n);
		t || Zt.set(n, t = []), t.push(e);
	}
}
function en(e, n, i = t) {
	let { immediate: a, deep: o, once: s, scheduler: l, augmentJob: u, call: f } = i, p = (e) => o ? e : /* @__PURE__ */ F(e) || o === !1 || o === 0 ? tn(e, 1) : tn(e), m, g, _, v, y = !1, b = !1;
	if (/* @__PURE__ */ L(e) ? (g = () => e.value, y = /* @__PURE__ */ F(e)) : /* @__PURE__ */ Lt(e) ? (g = () => p(e), y = !0) : d(e) ? (b = !0, y = e.some((e) => /* @__PURE__ */ Lt(e) || /* @__PURE__ */ F(e)), g = () => e.map((e) => {
		if (/* @__PURE__ */ L(e)) return e.value;
		if (/* @__PURE__ */ Lt(e)) return p(e);
		if (h(e)) return f ? f(e, 2) : e();
	})) : g = h(e) ? n ? f ? () => f(e, 2) : e : () => {
		if (_) {
			Ve();
			try {
				_();
			} finally {
				He();
			}
		}
		let t = Qt;
		Qt = m;
		try {
			return f ? f(e, 3, [v]) : e(v);
		} finally {
			Qt = t;
		}
	} : r, n && o) {
		let e = g, t = o === !0 ? Infinity : o;
		g = () => tn(e(), t);
	}
	let x = Te(), S = () => {
		m.stop(), x && x.active && c(x.effects, m);
	};
	if (s && n) {
		let e = n;
		n = (...t) => {
			e(...t), S();
		};
	}
	let C = b ? Array(e.length).fill(Xt) : Xt, w = (e) => {
		if (!(!(m.flags & 1) || !m.dirty && !e)) if (n) {
			let e = m.run();
			if (o || y || (b ? e.some((e, t) => D(e, C[t])) : D(e, C))) {
				_ && _();
				let t = Qt;
				Qt = m;
				try {
					let t = [
						e,
						C === Xt ? void 0 : b && C[0] === Xt ? [] : C,
						v
					];
					C = e, f ? f(n, 3, t) : n(...t);
				} finally {
					Qt = t;
				}
			}
		} else m.run();
	};
	return u && u(w), m = new De(g), m.scheduler = l ? () => l(w, !1) : w, v = (e) => $t(e, !1, m), _ = m.onStop = () => {
		let e = Zt.get(m);
		if (e) {
			if (f) f(e, 4);
			else for (let t of e) t();
			Zt.delete(m);
		}
	}, n ? a ? w(!0) : C = m.run() : l ? l(w.bind(null, !0), !0) : m.run(), S.pause = m.pause.bind(m), S.resume = m.resume.bind(m), S.stop = S, S;
}
function tn(e, t = Infinity, n) {
	if (t <= 0 || !v(e) || e.__v_skip || (n ||= /* @__PURE__ */ new Map(), (n.get(e) || 0) >= t)) return e;
	if (n.set(e, t), t--, /* @__PURE__ */ L(e)) tn(e.value, t, n);
	else if (d(e)) for (let r = 0; r < e.length; r++) tn(e[r], t, n);
	else if (p(e) || f(e)) e.forEach((e) => {
		tn(e, t, n);
	});
	else if (C(e)) {
		for (let r in e) tn(e[r], t, n);
		for (let r of Object.getOwnPropertySymbols(e)) Object.prototype.propertyIsEnumerable.call(e, r) && tn(e[r], t, n);
	}
	return e;
}
//#endregion
//#region node_modules/@vue/runtime-core/dist/runtime-core.esm-bundler.js
function nn(e, t, n, r) {
	try {
		return r ? e(...r) : e();
	} catch (e) {
		an(e, t, n);
	}
}
function rn(e, t, n, r) {
	if (h(e)) {
		let i = nn(e, t, n, r);
		return i && y(i) && i.catch((e) => {
			an(e, t, n);
		}), i;
	}
	if (d(e)) {
		let i = [];
		for (let a = 0; a < e.length; a++) i.push(rn(e[a], t, n, r));
		return i;
	}
}
function an(e, n, r, i = !0) {
	let a = n ? n.vnode : null, { errorHandler: o, throwUnhandledErrorInProduction: s } = n && n.appContext.config || t;
	if (n) {
		let t = n.parent, i = n.proxy, a = `https://vuejs.org/error-reference/#runtime-${r}`;
		for (; t;) {
			let n = t.ec;
			if (n) {
				for (let t = 0; t < n.length; t++) if (n[t](e, i, a) === !1) return;
			}
			t = t.parent;
		}
		if (o) {
			Ve(), nn(o, null, 10, [
				e,
				i,
				a
			]), He();
			return;
		}
	}
	on(e, r, a, i, s);
}
function on(e, t, n, r = !0, i = !1) {
	if (i) throw e;
	console.error(e);
}
var z = [], sn = -1, cn = [], ln = null, un = 0, dn = /* @__PURE__ */ Promise.resolve(), fn = null;
function pn(e) {
	let t = fn || dn;
	return e ? t.then(this ? e.bind(this) : e) : t;
}
function mn(e) {
	let t = sn + 1, n = z.length;
	for (; t < n;) {
		let r = t + n >>> 1, i = z[r], a = bn(i);
		a < e || a === e && i.flags & 2 ? t = r + 1 : n = r;
	}
	return t;
}
function hn(e) {
	if (!(e.flags & 1)) {
		let t = bn(e), n = z[z.length - 1];
		!n || !(e.flags & 2) && t >= bn(n) ? z.push(e) : z.splice(mn(t), 0, e), e.flags |= 1, gn();
	}
}
function gn() {
	fn ||= dn.then(xn);
}
function _n(e) {
	d(e) ? cn.push(...e) : ln && e.id === -1 ? ln.splice(un + 1, 0, e) : e.flags & 1 || (cn.push(e), e.flags |= 1), gn();
}
function vn(e, t, n = sn + 1) {
	for (; n < z.length; n++) {
		let t = z[n];
		if (t && t.flags & 2) {
			if (e && t.id !== e.uid) continue;
			z.splice(n, 1), n--, t.flags & 4 && (t.flags &= -2), t(), t.flags & 4 || (t.flags &= -2);
		}
	}
}
function yn(e) {
	if (cn.length) {
		let e = [...new Set(cn)].sort((e, t) => bn(e) - bn(t));
		if (cn.length = 0, ln) {
			ln.push(...e);
			return;
		}
		for (ln = e, un = 0; un < ln.length; un++) {
			let e = ln[un];
			e.flags & 4 && (e.flags &= -2), e.flags & 8 || e(), e.flags &= -2;
		}
		ln = null, un = 0;
	}
}
var bn = (e) => e.id == null ? e.flags & 2 ? -1 : Infinity : e.id;
function xn(e) {
	try {
		for (sn = 0; sn < z.length; sn++) {
			let e = z[sn];
			e && !(e.flags & 8) && (e.flags & 4 && (e.flags &= -2), nn(e, e.i, e.i ? 15 : 14), e.flags & 4 || (e.flags &= -2));
		}
	} finally {
		for (; sn < z.length; sn++) {
			let e = z[sn];
			e && (e.flags &= -2);
		}
		sn = -1, z.length = 0, yn(e), fn = null, (z.length || cn.length) && xn(e);
	}
}
var B = null, Sn = null;
function Cn(e) {
	let t = B;
	return B = e, Sn = e && e.type.__scopeId || null, t;
}
function wn(e, t = B, n) {
	if (!t || e._n) return e;
	let r = (...n) => {
		r._d && Ri(-1);
		let i = Cn(t), a;
		try {
			a = e(...n);
		} finally {
			Cn(i), r._d && Ri(1);
		}
		return a;
	};
	return r._n = !0, r._c = !0, r._d = !0, r;
}
function V(e, n) {
	if (B === null) return e;
	let r = va(B), i = e.dirs ||= [];
	for (let e = 0; e < n.length; e++) {
		let [a, o, s, c = t] = n[e];
		a && (h(a) && (a = {
			mounted: a,
			updated: a
		}), a.deep && tn(o), i.push({
			dir: a,
			instance: r,
			value: o,
			oldValue: void 0,
			arg: s,
			modifiers: c
		}));
	}
	return e;
}
function Tn(e, t, n, r) {
	let i = e.dirs, a = t && t.dirs;
	for (let o = 0; o < i.length; o++) {
		let s = i[o];
		a && (s.oldValue = a[o].value);
		let c = s.dir[r];
		c && (Ve(), rn(c, n, 8, [
			e.el,
			s,
			e,
			t
		]), He());
	}
}
function En(e, t) {
	if (Z) {
		let n = Z.provides, r = Z.parent && Z.parent.provides;
		r === n && (n = Z.provides = Object.create(r)), n[e] = t;
	}
}
function Dn(e, t, n = !1) {
	let r = ra();
	if (r || Ur) {
		let i = Ur ? Ur._context.provides : r ? r.parent == null || r.ce ? r.vnode.appContext && r.vnode.appContext.provides : r.parent.provides : void 0;
		if (i && e in i) return i[e];
		if (arguments.length > 1) return n && h(t) ? t.call(r && r.proxy) : t;
	}
}
var On = /* @__PURE__ */ Symbol.for("v-scx"), kn = () => Dn(On);
function An(e, t, n) {
	return jn(e, t, n);
}
function jn(e, n, i = t) {
	let { immediate: a, deep: o, flush: c, once: l } = i, u = s({}, i), d = n && a || !n && c !== "post", f;
	if (la) {
		if (c === "sync") {
			let e = kn();
			f = e.__watcherHandles ||= [];
		} else if (!d) {
			let e = () => {};
			return e.stop = r, e.resume = r, e.pause = r, e;
		}
	}
	let p = Z;
	u.call = (e, t, n) => rn(e, p, t, n);
	let m = !1;
	c === "post" ? u.scheduler = (e) => {
		U(e, p && p.suspense);
	} : c !== "sync" && (m = !0, u.scheduler = (e, t) => {
		t ? e() : hn(e);
	}), u.augmentJob = (e) => {
		n && (e.flags |= 4), m && (e.flags |= 2, p && (e.id = p.uid, e.i = p));
	};
	let h = en(e, n, u);
	return la && (f ? f.push(h) : d && h()), h;
}
function Mn(e, t, n) {
	let r = this.proxy, i = g(e) ? e.includes(".") ? Nn(r, e) : () => r[e] : e.bind(r, r), a;
	h(t) ? a = t : (a = t.handler, n = t);
	let o = oa(this), s = jn(i, a.bind(r), n);
	return o(), s;
}
function Nn(e, t) {
	let n = t.split(".");
	return () => {
		let t = e;
		for (let e = 0; e < n.length && t; e++) t = t[n[e]];
		return t;
	};
}
var Pn = /* @__PURE__ */ new WeakMap(), Fn = /* @__PURE__ */ Symbol("_vte"), In = (e) => e.__isTeleport, Ln = (e) => e && (e.disabled || e.disabled === ""), Rn = (e) => e && (e.defer || e.defer === ""), zn = (e) => typeof SVGElement < "u" && e instanceof SVGElement, Bn = (e) => typeof MathMLElement == "function" && e instanceof MathMLElement, Vn = (e, t) => {
	let n = e && e.to;
	return g(n) ? t ? t(n) : null : n;
}, Hn = {
	name: "Teleport",
	__isTeleport: !0,
	process(e, t, n, r, i, a, o, s, c, l) {
		let { mc: u, pc: d, pbc: f, o: { insert: p, querySelector: m, createText: h, createComment: g, parentNode: _ } } = l, v = Ln(t.props), { dynamicChildren: y } = t, b = (e, t, n) => {
			e.shapeFlag & 16 && u(e.children, t, n, i, a, o, s, c);
		}, x = (e = t) => {
			let n = Ln(e.props), r = e.target = Vn(e.props, m), a = qn(r, e, h, p);
			r && (o !== "svg" && zn(r) ? o = "svg" : o !== "mathml" && Bn(r) && (o = "mathml"), i && i.isCE && (i.ce._teleportTargets || (i.ce._teleportTargets = /* @__PURE__ */ new Set())).add(r), n || (b(e, r, a), Kn(e, !1)));
		}, S = (e) => {
			let t = () => {
				Pn.get(e) === t && (Pn.delete(e), Ln(e.props) && (b(e, _(e.el) || n, e.anchor), Kn(e, !0)), x(e));
			};
			Pn.set(e, t), U(t, a);
		};
		if (e == null) {
			let e = t.el = h(""), i = t.anchor = h("");
			if (p(e, n, r), p(i, n, r), Rn(t.props) || a && a.pendingBranch) {
				S(t);
				return;
			}
			v && (b(t, n, i), Kn(t, !0)), x();
		} else {
			t.el = e.el;
			let r = t.anchor = e.anchor, u = Pn.get(e);
			if (u) {
				u.flags |= 8, Pn.delete(e), S(t);
				return;
			}
			t.targetStart = e.targetStart;
			let p = t.target = e.target, h = t.targetAnchor = e.targetAnchor, g = Ln(e.props), _ = g ? n : p, b = g ? r : h;
			if (o === "svg" || zn(p) ? o = "svg" : (o === "mathml" || Bn(p)) && (o = "mathml"), y ? (f(e.dynamicChildren, y, _, i, a, o, s), Ti(e, t, !0)) : c || d(e, t, _, b, i, a, o, s, !1), v) g ? t.props && e.props && t.props.to !== e.props.to && (t.props.to = e.props.to) : Un(t, n, r, l, 1);
			else if ((t.props && t.props.to) !== (e.props && e.props.to)) {
				let e = t.target = Vn(t.props, m);
				e && Un(t, e, null, l, 0);
			} else g && Un(t, p, h, l, 1);
			Kn(t, v);
		}
	},
	remove(e, t, n, { um: r, o: { remove: i } }, a) {
		let { shapeFlag: o, children: s, anchor: c, targetStart: l, targetAnchor: u, target: d, props: f } = e, p = a || !Ln(f), m = Pn.get(e);
		if (m && (m.flags |= 8, Pn.delete(e), p = !1), d && (i(l), i(u)), a && i(c), o & 16) for (let e = 0; e < s.length; e++) {
			let i = s[e];
			r(i, t, n, p, !!i.dynamicChildren);
		}
	},
	move: Un,
	hydrate: Wn
};
function Un(e, t, n, { o: { insert: r }, m: i }, a = 2) {
	a === 0 && r(e.targetAnchor, t, n);
	let { el: o, anchor: s, shapeFlag: c, children: l, props: u } = e, d = a === 2;
	if (d && r(o, t, n), !Pn.has(e) && (!d || Ln(u)) && c & 16) for (let e = 0; e < l.length; e++) i(l[e], t, n, 2);
	d && r(s, t, n);
}
function Wn(e, t, n, r, i, a, { o: { nextSibling: o, parentNode: s, querySelector: c, insert: l, createText: u } }, d) {
	function f(e, n) {
		let r = n;
		for (; r;) {
			if (r && r.nodeType === 8) {
				if (r.data === "teleport start anchor") t.targetStart = r;
				else if (r.data === "teleport anchor") {
					t.targetAnchor = r, e._lpa = t.targetAnchor && o(t.targetAnchor);
					break;
				}
			}
			r = o(r);
		}
	}
	function p(e, t) {
		t.anchor = d(o(e), t, s(e), n, r, i, a);
	}
	let m = t.target = Vn(t.props, c), h = Ln(t.props);
	if (m) {
		let c = m._lpa || m.firstChild;
		t.shapeFlag & 16 && (h ? (p(e, t), f(m, c), t.targetAnchor || qn(m, t, u, l, s(e) === m ? e : null)) : (t.anchor = o(e), f(m, c), t.targetAnchor || qn(m, t, u, l), d(c && o(c), t, m, n, r, i, a))), Kn(t, h);
	} else h && t.shapeFlag & 16 && (p(e, t), t.targetStart = e, t.targetAnchor = o(e));
	return t.anchor && o(t.anchor);
}
var Gn = Hn;
function Kn(e, t) {
	let n = e.ctx;
	if (n && n.ut) {
		let r, i;
		for (t ? (r = e.el, i = e.anchor) : (r = e.targetStart, i = e.targetAnchor); r && r !== i;) r.nodeType === 1 && r.setAttribute("data-v-owner", n.uid), r = r.nextSibling;
		n.ut();
	}
}
function qn(e, t, n, r, i = null) {
	let a = t.targetStart = n(""), o = t.targetAnchor = n("");
	return a[Fn] = o, e && (r(a, e, i), r(o, e, i)), o;
}
var Jn = /* @__PURE__ */ Symbol("_leaveCb");
function Yn(e, t) {
	e.shapeFlag & 6 && e.component ? (e.transition = t, Yn(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
function Xn(e) {
	e.ids = [
		e.ids[0] + e.ids[2]++ + "-",
		0,
		0
	];
}
function Zn(e, t) {
	let n;
	return !!((n = Object.getOwnPropertyDescriptor(e, t)) && !n.configurable);
}
var Qn = /* @__PURE__ */ new WeakMap();
function $n(e, n, r, a, o = !1) {
	if (d(e)) {
		e.forEach((e, t) => $n(e, n && (d(n) ? n[t] : n), r, a, o));
		return;
	}
	if (tr(a) && !o) {
		a.shapeFlag & 512 && a.type.__asyncResolved && a.component.subTree.component && $n(e, n, r, a.component.subTree);
		return;
	}
	let s = a.shapeFlag & 4 ? va(a.component) : a.el, l = o ? null : s, { i: f, r: p } = e, m = n && n.r, _ = f.refs === t ? f.refs = {} : f.refs, v = f.setupState, y = /* @__PURE__ */ I(v), b = v === t ? i : (e) => Zn(_, e) ? !1 : u(y, e), x = (e, t) => !(t && Zn(_, t));
	if (m != null && m !== p) {
		if (er(n), g(m)) _[m] = null, b(m) && (v[m] = null);
		else if (/* @__PURE__ */ L(m)) {
			let e = n;
			x(m, e.k) && (m.value = null), e.k && (_[e.k] = null);
		}
	}
	if (h(p)) nn(p, f, 12, [l, _]);
	else {
		let t = g(p), n = /* @__PURE__ */ L(p);
		if (t || n) {
			let i = () => {
				if (e.f) {
					let n = t ? b(p) ? v[p] : _[p] : x(p) || !e.k ? p.value : _[e.k];
					if (o) d(n) && c(n, s);
					else if (d(n)) n.includes(s) || n.push(s);
					else if (t) _[p] = [s], b(p) && (v[p] = _[p]);
					else {
						let t = [s];
						x(p, e.k) && (p.value = t), e.k && (_[e.k] = t);
					}
				} else t ? (_[p] = l, b(p) && (v[p] = l)) : n && (x(p, e.k) && (p.value = l), e.k && (_[e.k] = l));
			};
			if (l) {
				let t = () => {
					i(), Qn.delete(e);
				};
				t.id = -1, Qn.set(e, t), U(t, r);
			} else er(e), i();
		}
	}
}
function er(e) {
	let t = Qn.get(e);
	t && (t.flags |= 8, Qn.delete(e));
}
le().requestIdleCallback, le().cancelIdleCallback;
var tr = (e) => !!e.type.__asyncLoader, nr = (e) => e.type.__isKeepAlive;
function rr(e, t) {
	ar(e, "a", t);
}
function ir(e, t) {
	ar(e, "da", t);
}
function ar(e, t, n = Z) {
	let r = e.__wdc ||= () => {
		let t = n;
		for (; t;) {
			if (t.isDeactivated) return;
			t = t.parent;
		}
		return e();
	};
	if (sr(t, r, n), n) {
		let e = n.parent;
		for (; e && e.parent;) nr(e.parent.vnode) && or(r, t, n, e), e = e.parent;
	}
}
function or(e, t, n, r) {
	let i = sr(t, e, r, !0);
	mr(() => {
		c(r[t], i);
	}, n);
}
function sr(e, t, n = Z, r = !1) {
	if (n) {
		let i = n[e] || (n[e] = []), a = t.__weh ||= (...r) => {
			Ve();
			let i = oa(n), a = rn(t, n, e, r);
			return i(), He(), a;
		};
		return r ? i.unshift(a) : i.push(a), a;
	}
}
var cr = (e) => (t, n = Z) => {
	(!la || e === "sp") && sr(e, (...e) => t(...e), n);
}, lr = cr("bm"), ur = cr("m"), dr = cr("bu"), fr = cr("u"), pr = cr("bum"), mr = cr("um"), hr = cr("sp"), gr = cr("rtg"), _r = cr("rtc");
function vr(e, t = Z) {
	sr("ec", e, t);
}
var yr = /* @__PURE__ */ Symbol.for("v-ndc");
function br(e, t, n, r) {
	let i, a = n && n[r], o = d(e);
	if (o || g(e)) {
		let n = o && /* @__PURE__ */ Lt(e), r = !1, s = !1;
		n && (r = !/* @__PURE__ */ F(e), s = /* @__PURE__ */ Rt(e), e = et(e)), i = Array(e.length);
		for (let n = 0, o = e.length; n < o; n++) i[n] = t(r ? s ? Ht(Vt(e[n])) : Vt(e[n]) : e[n], n, void 0, a && a[n]);
	} else if (typeof e == "number") {
		i = Array(e);
		for (let n = 0; n < e; n++) i[n] = t(n + 1, n, void 0, a && a[n]);
	} else if (v(e)) if (e[Symbol.iterator]) i = Array.from(e, (e, n) => t(e, n, void 0, a && a[n]));
	else {
		let n = Object.keys(e);
		i = Array(n.length);
		for (let r = 0, o = n.length; r < o; r++) {
			let o = n[r];
			i[r] = t(e[o], o, r, a && a[r]);
		}
	}
	else i = [];
	return n && (n[r] = i), i;
}
var xr = (e) => e ? ca(e) ? va(e) : xr(e.parent) : null, Sr = /* @__PURE__ */ s(/* @__PURE__ */ Object.create(null), {
	$: (e) => e,
	$el: (e) => e.vnode.el,
	$data: (e) => e.data,
	$props: (e) => e.props,
	$attrs: (e) => e.attrs,
	$slots: (e) => e.slots,
	$refs: (e) => e.refs,
	$parent: (e) => xr(e.parent),
	$root: (e) => xr(e.root),
	$host: (e) => e.ce,
	$emit: (e) => e.emit,
	$options: (e) => jr(e),
	$forceUpdate: (e) => e.f ||= () => {
		hn(e.update);
	},
	$nextTick: (e) => e.n ||= pn.bind(e.proxy),
	$watch: (e) => Mn.bind(e)
}), Cr = (e, n) => e !== t && !e.__isScriptSetup && u(e, n), wr = {
	get({ _: e }, n) {
		if (n === "__v_skip") return !0;
		let { ctx: r, setupState: i, data: a, props: o, accessCache: s, type: c, appContext: l } = e;
		if (n[0] !== "$") {
			let e = s[n];
			if (e !== void 0) switch (e) {
				case 1: return i[n];
				case 2: return a[n];
				case 4: return r[n];
				case 3: return o[n];
			}
			else if (Cr(i, n)) return s[n] = 1, i[n];
			else if (a !== t && u(a, n)) return s[n] = 2, a[n];
			else if (u(o, n)) return s[n] = 3, o[n];
			else if (r !== t && u(r, n)) return s[n] = 4, r[n];
			else Er && (s[n] = 0);
		}
		let d = Sr[n], f, p;
		if (d) return n === "$attrs" && P(e.attrs, "get", ""), d(e);
		if ((f = c.__cssModules) && (f = f[n])) return f;
		if (r !== t && u(r, n)) return s[n] = 4, r[n];
		if (p = l.config.globalProperties, u(p, n)) return p[n];
	},
	set({ _: e }, n, r) {
		let { data: i, setupState: a, ctx: o } = e;
		return Cr(a, n) ? (a[n] = r, !0) : i !== t && u(i, n) ? (i[n] = r, !0) : u(e.props, n) || n[0] === "$" && n.slice(1) in e ? !1 : (o[n] = r, !0);
	},
	has({ _: { data: e, setupState: n, accessCache: r, ctx: i, appContext: a, props: o, type: s } }, c) {
		let l;
		return !!(r[c] || e !== t && c[0] !== "$" && u(e, c) || Cr(n, c) || u(o, c) || u(i, c) || u(Sr, c) || u(a.config.globalProperties, c) || (l = s.__cssModules) && l[c]);
	},
	defineProperty(e, t, n) {
		return n.get == null ? u(n, "value") && this.set(e, t, n.value, null) : e._.accessCache[t] = 0, Reflect.defineProperty(e, t, n);
	}
};
function Tr(e) {
	return d(e) ? e.reduce((e, t) => (e[t] = null, e), {}) : e;
}
var Er = !0;
function Dr(e) {
	let t = jr(e), n = e.proxy, i = e.ctx;
	Er = !1, t.beforeCreate && kr(t.beforeCreate, e, "bc");
	let { data: a, computed: o, methods: s, watch: c, provide: l, inject: u, created: f, beforeMount: p, mounted: m, beforeUpdate: g, updated: _, activated: y, deactivated: b, beforeDestroy: x, beforeUnmount: S, destroyed: C, unmounted: w, render: ee, renderTracked: te, renderTriggered: ne, errorCaptured: T, serverPrefetch: re, expose: E, inheritAttrs: ie, components: ae, directives: D, filters: oe } = t;
	if (u && Or(u, i, null), s) for (let e in s) {
		let t = s[e];
		h(t) && (i[e] = t.bind(n));
	}
	if (a) {
		let t = a.call(n, n);
		v(t) && (e.data = /* @__PURE__ */ Nt(t));
	}
	if (Er = !0, o) for (let e in o) {
		let t = o[e], a = ba({
			get: h(t) ? t.bind(n, n) : h(t.get) ? t.get.bind(n, n) : r,
			set: !h(t) && h(t.set) ? t.set.bind(n) : r
		});
		Object.defineProperty(i, e, {
			enumerable: !0,
			configurable: !0,
			get: () => a.value,
			set: (e) => a.value = e
		});
	}
	if (c) for (let e in c) Ar(c[e], i, n, e);
	if (l) {
		let e = h(l) ? l.call(n) : l;
		Reflect.ownKeys(e).forEach((t) => {
			En(t, e[t]);
		});
	}
	f && kr(f, e, "c");
	function O(e, t) {
		d(t) ? t.forEach((t) => e(t.bind(n))) : t && e(t.bind(n));
	}
	if (O(lr, p), O(ur, m), O(dr, g), O(fr, _), O(rr, y), O(ir, b), O(vr, T), O(_r, te), O(gr, ne), O(pr, S), O(mr, w), O(hr, re), d(E)) if (E.length) {
		let t = e.exposed ||= {};
		E.forEach((e) => {
			Object.defineProperty(t, e, {
				get: () => n[e],
				set: (t) => n[e] = t,
				enumerable: !0
			});
		});
	} else e.exposed ||= {};
	ee && e.render === r && (e.render = ee), ie != null && (e.inheritAttrs = ie), ae && (e.components = ae), D && (e.directives = D), re && Xn(e);
}
function Or(e, t, n = r) {
	d(e) && (e = Ir(e));
	for (let n in e) {
		let r = e[n], i;
		i = v(r) ? "default" in r ? Dn(r.from || n, r.default, !0) : Dn(r.from || n) : Dn(r), /* @__PURE__ */ L(i) ? Object.defineProperty(t, n, {
			enumerable: !0,
			configurable: !0,
			get: () => i.value,
			set: (e) => i.value = e
		}) : t[n] = i;
	}
}
function kr(e, t, n) {
	rn(d(e) ? e.map((e) => e.bind(t.proxy)) : e.bind(t.proxy), t, n);
}
function Ar(e, t, n, r) {
	let i = r.includes(".") ? Nn(n, r) : () => n[r];
	if (g(e)) {
		let n = t[e];
		h(n) && An(i, n);
	} else if (h(e)) An(i, e.bind(n));
	else if (v(e)) if (d(e)) e.forEach((e) => Ar(e, t, n, r));
	else {
		let r = h(e.handler) ? e.handler.bind(n) : t[e.handler];
		h(r) && An(i, r, e);
	}
}
function jr(e) {
	let t = e.type, { mixins: n, extends: r } = t, { mixins: i, optionsCache: a, config: { optionMergeStrategies: o } } = e.appContext, s = a.get(t), c;
	return s ? c = s : !i.length && !n && !r ? c = t : (c = {}, i.length && i.forEach((e) => Mr(c, e, o, !0)), Mr(c, t, o)), v(t) && a.set(t, c), c;
}
function Mr(e, t, n, r = !1) {
	let { mixins: i, extends: a } = t;
	a && Mr(e, a, n, !0), i && i.forEach((t) => Mr(e, t, n, !0));
	for (let i in t) if (!(r && i === "expose")) {
		let r = Nr[i] || n && n[i];
		e[i] = r ? r(e[i], t[i]) : t[i];
	}
	return e;
}
var Nr = {
	data: Pr,
	props: Rr,
	emits: Rr,
	methods: Lr,
	computed: Lr,
	beforeCreate: H,
	created: H,
	beforeMount: H,
	mounted: H,
	beforeUpdate: H,
	updated: H,
	beforeDestroy: H,
	beforeUnmount: H,
	destroyed: H,
	unmounted: H,
	activated: H,
	deactivated: H,
	errorCaptured: H,
	serverPrefetch: H,
	components: Lr,
	directives: Lr,
	watch: zr,
	provide: Pr,
	inject: Fr
};
function Pr(e, t) {
	return t ? e ? function() {
		return s(h(e) ? e.call(this, this) : e, h(t) ? t.call(this, this) : t);
	} : t : e;
}
function Fr(e, t) {
	return Lr(Ir(e), Ir(t));
}
function Ir(e) {
	if (d(e)) {
		let t = {};
		for (let n = 0; n < e.length; n++) t[e[n]] = e[n];
		return t;
	}
	return e;
}
function H(e, t) {
	return e ? [...new Set([].concat(e, t))] : t;
}
function Lr(e, t) {
	return e ? s(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function Rr(e, t) {
	return e ? d(e) && d(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : s(/* @__PURE__ */ Object.create(null), Tr(e), Tr(t ?? {})) : t;
}
function zr(e, t) {
	if (!e) return t;
	if (!t) return e;
	let n = s(/* @__PURE__ */ Object.create(null), e);
	for (let r in t) n[r] = H(e[r], t[r]);
	return n;
}
function Br() {
	return {
		app: null,
		config: {
			isNativeTag: i,
			performance: !1,
			globalProperties: {},
			optionMergeStrategies: {},
			errorHandler: void 0,
			warnHandler: void 0,
			compilerOptions: {}
		},
		mixins: [],
		components: {},
		directives: {},
		provides: /* @__PURE__ */ Object.create(null),
		optionsCache: /* @__PURE__ */ new WeakMap(),
		propsCache: /* @__PURE__ */ new WeakMap(),
		emitsCache: /* @__PURE__ */ new WeakMap()
	};
}
var Vr = 0;
function Hr(e, t) {
	return function(n, r = null) {
		h(n) || (n = s({}, n)), r != null && !v(r) && (r = null);
		let i = Br(), a = /* @__PURE__ */ new WeakSet(), o = [], c = !1, l = i.app = {
			_uid: Vr++,
			_component: n,
			_props: r,
			_container: null,
			_context: i,
			_instance: null,
			version: xa,
			get config() {
				return i.config;
			},
			set config(e) {},
			use(e, ...t) {
				return a.has(e) || (e && h(e.install) ? (a.add(e), e.install(l, ...t)) : h(e) && (a.add(e), e(l, ...t))), l;
			},
			mixin(e) {
				return i.mixins.includes(e) || i.mixins.push(e), l;
			},
			component(e, t) {
				return t ? (i.components[e] = t, l) : i.components[e];
			},
			directive(e, t) {
				return t ? (i.directives[e] = t, l) : i.directives[e];
			},
			mount(a, o, s) {
				if (!c) {
					let u = l._ceVNode || Gi(n, r);
					return u.appContext = i, s === !0 ? s = "svg" : s === !1 && (s = void 0), o && t ? t(u, a) : e(u, a, s), c = !0, l._container = a, a.__vue_app__ = l, va(u.component);
				}
			},
			onUnmount(e) {
				o.push(e);
			},
			unmount() {
				c && (rn(o, l._instance, 16), e(null, l._container), delete l._container.__vue_app__);
			},
			provide(e, t) {
				return i.provides[e] = t, l;
			},
			runWithContext(e) {
				let t = Ur;
				Ur = l;
				try {
					return e();
				} finally {
					Ur = t;
				}
			}
		};
		return l;
	};
}
var Ur = null, Wr = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${T(t)}Modifiers`] || e[`${E(t)}Modifiers`];
function Gr(e, n, ...r) {
	if (e.isUnmounted) return;
	let i = e.vnode.props || t, a = r, o = n.startsWith("update:"), s = o && Wr(i, n.slice(7));
	s && (s.trim && (a = r.map((e) => g(e) ? e.trim() : e)), s.number && (a = r.map(se)));
	let c, l = i[c = ae(n)] || i[c = ae(T(n))];
	!l && o && (l = i[c = ae(E(n))]), l && rn(l, e, 6, a);
	let u = i[c + "Once"];
	if (u) {
		if (!e.emitted) e.emitted = {};
		else if (e.emitted[c]) return;
		e.emitted[c] = !0, rn(u, e, 6, a);
	}
}
var Kr = /* @__PURE__ */ new WeakMap();
function qr(e, t, n = !1) {
	let r = n ? Kr : t.emitsCache, i = r.get(e);
	if (i !== void 0) return i;
	let a = e.emits, o = {}, c = !1;
	if (!h(e)) {
		let r = (e) => {
			let n = qr(e, t, !0);
			n && (c = !0, s(o, n));
		};
		!n && t.mixins.length && t.mixins.forEach(r), e.extends && r(e.extends), e.mixins && e.mixins.forEach(r);
	}
	return !a && !c ? (v(e) && r.set(e, null), null) : (d(a) ? a.forEach((e) => o[e] = null) : s(o, a), v(e) && r.set(e, o), o);
}
function Jr(e, t) {
	return !e || !a(t) ? !1 : (t = t.slice(2).replace(/Once$/, ""), u(e, t[0].toLowerCase() + t.slice(1)) || u(e, E(t)) || u(e, t));
}
function Yr(e) {
	let { type: t, vnode: n, proxy: r, withProxy: i, propsOptions: [a], slots: s, attrs: c, emit: l, render: u, renderCache: d, props: f, data: p, setupState: m, ctx: h, inheritAttrs: g } = e, _ = Cn(e), v, y;
	try {
		if (n.shapeFlag & 4) {
			let e = i || r, t = e;
			v = Yi(u.call(t, e, d, f, m, p, h)), y = c;
		} else {
			let e = t;
			v = Yi(e.length > 1 ? e(f, {
				attrs: c,
				slots: s,
				emit: l
			}) : e(f, null)), y = t.props ? c : Xr(c);
		}
	} catch (t) {
		Fi.length = 0, an(t, e, 1), v = Gi(Ni);
	}
	let b = v;
	if (y && g !== !1) {
		let e = Object.keys(y), { shapeFlag: t } = b;
		e.length && t & 7 && (a && e.some(o) && (y = Zr(y, a)), b = Ji(b, y, !1, !0));
	}
	return n.dirs && (b = Ji(b, null, !1, !0), b.dirs = b.dirs ? b.dirs.concat(n.dirs) : n.dirs), n.transition && Yn(b, n.transition), v = b, Cn(_), v;
}
var Xr = (e) => {
	let t;
	for (let n in e) (n === "class" || n === "style" || a(n)) && ((t ||= {})[n] = e[n]);
	return t;
}, Zr = (e, t) => {
	let n = {};
	for (let r in e) (!o(r) || !(r.slice(9) in t)) && (n[r] = e[r]);
	return n;
};
function Qr(e, t, n) {
	let { props: r, children: i, component: a } = e, { props: o, children: s, patchFlag: c } = t, l = a.emitsOptions;
	if (t.dirs || t.transition) return !0;
	if (n && c >= 0) {
		if (c & 1024) return !0;
		if (c & 16) return r ? $r(r, o, l) : !!o;
		if (c & 8) {
			let e = t.dynamicProps;
			for (let t = 0; t < e.length; t++) {
				let n = e[t];
				if (ei(o, r, n) && !Jr(l, n)) return !0;
			}
		}
	} else return (i || s) && (!s || !s.$stable) ? !0 : r === o ? !1 : r ? o ? $r(r, o, l) : !0 : !!o;
	return !1;
}
function $r(e, t, n) {
	let r = Object.keys(t);
	if (r.length !== Object.keys(e).length) return !0;
	for (let i = 0; i < r.length; i++) {
		let a = r[i];
		if (ei(t, e, a) && !Jr(n, a)) return !0;
	}
	return !1;
}
function ei(e, t, n) {
	let r = e[n], i = t[n];
	return n === "style" && v(r) && v(i) ? !ye(r, i) : r !== i;
}
function ti({ vnode: e, parent: t, suspense: n }, r) {
	for (; t;) {
		let n = t.subTree;
		if (n.suspense && n.suspense.activeBranch === e && (n.suspense.vnode.el = n.el = r, e = n), n === e) (e = t.vnode).el = r, t = t.parent;
		else break;
	}
	n && n.activeBranch === e && (n.vnode.el = r);
}
var ni = {}, ri = () => Object.create(ni), ii = (e) => Object.getPrototypeOf(e) === ni;
function ai(e, t, n, r = !1) {
	let i = {}, a = ri();
	e.propsDefaults = /* @__PURE__ */ Object.create(null), si(e, t, i, a);
	for (let t in e.propsOptions[0]) t in i || (i[t] = void 0);
	n ? e.props = r ? i : /* @__PURE__ */ Pt(i) : e.type.props ? e.props = i : e.props = a, e.attrs = a;
}
function oi(e, t, n, r) {
	let { props: i, attrs: a, vnode: { patchFlag: o } } = e, s = /* @__PURE__ */ I(i), [c] = e.propsOptions, l = !1;
	if ((r || o > 0) && !(o & 16)) {
		if (o & 8) {
			let n = e.vnode.dynamicProps;
			for (let r = 0; r < n.length; r++) {
				let o = n[r];
				if (Jr(e.emitsOptions, o)) continue;
				let d = t[o];
				if (c) if (u(a, o)) d !== a[o] && (a[o] = d, l = !0);
				else {
					let t = T(o);
					i[t] = ci(c, s, t, d, e, !1);
				}
				else d !== a[o] && (a[o] = d, l = !0);
			}
		}
	} else {
		si(e, t, i, a) && (l = !0);
		let r;
		for (let a in s) (!t || !u(t, a) && ((r = E(a)) === a || !u(t, r))) && (c ? n && (n[a] !== void 0 || n[r] !== void 0) && (i[a] = ci(c, s, a, void 0, e, !0)) : delete i[a]);
		if (a !== s) for (let e in a) (!t || !u(t, e)) && (delete a[e], l = !0);
	}
	l && Qe(e.attrs, "set", "");
}
function si(e, n, r, i) {
	let [a, o] = e.propsOptions, s = !1, c;
	if (n) for (let t in n) {
		if (ee(t)) continue;
		let l = n[t], d;
		a && u(a, d = T(t)) ? !o || !o.includes(d) ? r[d] = l : (c ||= {})[d] = l : Jr(e.emitsOptions, t) || (!(t in i) || l !== i[t]) && (i[t] = l, s = !0);
	}
	if (o) {
		let n = /* @__PURE__ */ I(r), i = c || t;
		for (let t = 0; t < o.length; t++) {
			let s = o[t];
			r[s] = ci(a, n, s, i[s], e, !u(i, s));
		}
	}
	return s;
}
function ci(e, t, n, r, i, a) {
	let o = e[n];
	if (o != null) {
		let e = u(o, "default");
		if (e && r === void 0) {
			let e = o.default;
			if (o.type !== Function && !o.skipFactory && h(e)) {
				let { propsDefaults: a } = i;
				if (n in a) r = a[n];
				else {
					let o = oa(i);
					r = a[n] = e.call(null, t), o();
				}
			} else r = e;
			i.ce && i.ce._setProp(n, r);
		}
		o[0] && (a && !e ? r = !1 : o[1] && (r === "" || r === E(n)) && (r = !0));
	}
	return r;
}
var li = /* @__PURE__ */ new WeakMap();
function ui(e, r, i = !1) {
	let a = i ? li : r.propsCache, o = a.get(e);
	if (o) return o;
	let c = e.props, l = {}, f = [], p = !1;
	if (!h(e)) {
		let t = (e) => {
			p = !0;
			let [t, n] = ui(e, r, !0);
			s(l, t), n && f.push(...n);
		};
		!i && r.mixins.length && r.mixins.forEach(t), e.extends && t(e.extends), e.mixins && e.mixins.forEach(t);
	}
	if (!c && !p) return v(e) && a.set(e, n), n;
	if (d(c)) for (let e = 0; e < c.length; e++) {
		let n = T(c[e]);
		di(n) && (l[n] = t);
	}
	else if (c) for (let e in c) {
		let t = T(e);
		if (di(t)) {
			let n = c[e], r = l[t] = d(n) || h(n) ? { type: n } : s({}, n), i = r.type, a = !1, o = !0;
			if (d(i)) for (let e = 0; e < i.length; ++e) {
				let t = i[e], n = h(t) && t.name;
				if (n === "Boolean") {
					a = !0;
					break;
				} else n === "String" && (o = !1);
			}
			else a = h(i) && i.name === "Boolean";
			r[0] = a, r[1] = o, (a || u(r, "default")) && f.push(t);
		}
	}
	let m = [l, f];
	return v(e) && a.set(e, m), m;
}
function di(e) {
	return e[0] !== "$" && !ee(e);
}
var fi = (e) => e === "_" || e === "_ctx" || e === "$stable", pi = (e) => d(e) ? e.map(Yi) : [Yi(e)], mi = (e, t, n) => {
	if (t._n) return t;
	let r = wn((...e) => pi(t(...e)), n);
	return r._c = !1, r;
}, hi = (e, t, n) => {
	let r = e._ctx;
	for (let n in e) {
		if (fi(n)) continue;
		let i = e[n];
		if (h(i)) t[n] = mi(n, i, r);
		else if (i != null) {
			let e = pi(i);
			t[n] = () => e;
		}
	}
}, gi = (e, t) => {
	let n = pi(t);
	e.slots.default = () => n;
}, _i = (e, t, n) => {
	for (let r in t) (n || !fi(r)) && (e[r] = t[r]);
}, vi = (e, t, n) => {
	let r = e.slots = ri();
	if (e.vnode.shapeFlag & 32) {
		let e = t._;
		e ? (_i(r, t, n), n && O(r, "_", e, !0)) : hi(t, r);
	} else t && gi(e, t);
}, yi = (e, n, r) => {
	let { vnode: i, slots: a } = e, o = !0, s = t;
	if (i.shapeFlag & 32) {
		let e = n._;
		e ? r && e === 1 ? o = !1 : _i(a, n, r) : (o = !n.$stable, hi(n, a)), s = n;
	} else n && (gi(e, n), s = { default: 1 });
	if (o) for (let e in a) !fi(e) && s[e] == null && delete a[e];
}, U = ji;
function bi(e) {
	return xi(e);
}
function xi(e, i) {
	let a = le();
	a.__VUE__ = !0;
	let { insert: o, remove: s, patchProp: c, createElement: l, createText: u, createComment: d, setText: f, setElementText: p, parentNode: m, nextSibling: h, setScopeId: g = r, insertStaticContent: _ } = e, v = (e, t, n, r = null, i = null, a = null, o = void 0, s = null, c = !!t.dynamicChildren) => {
		if (e === t) return;
		e && !Hi(e, t) && (r = ye(e), k(e, i, a, !0), e = null), t.patchFlag === -2 && (c = !1, t.dynamicChildren = null);
		let { type: l, ref: u, shapeFlag: d } = t;
		switch (l) {
			case Mi:
				y(e, t, n, r);
				break;
			case Ni:
				b(e, t, n, r);
				break;
			case Pi:
				e ?? x(t, n, r, o);
				break;
			case W:
				ae(e, t, n, r, i, a, o, s, c);
				break;
			default: d & 1 ? w(e, t, n, r, i, a, o, s, c) : d & 6 ? D(e, t, n, r, i, a, o, s, c) : (d & 64 || d & 128) && l.process(e, t, n, r, i, a, o, s, c, A);
		}
		u != null && i ? $n(u, e && e.ref, a, t || e, !t) : u == null && e && e.ref != null && $n(e.ref, null, a, e, !0);
	}, y = (e, t, n, r) => {
		if (e == null) o(t.el = u(t.children), n, r);
		else {
			let n = t.el = e.el;
			t.children !== e.children && f(n, t.children);
		}
	}, b = (e, t, n, r) => {
		e == null ? o(t.el = d(t.children || ""), n, r) : t.el = e.el;
	}, x = (e, t, n, r) => {
		[e.el, e.anchor] = _(e.children, t, n, r, e.el, e.anchor);
	}, S = ({ el: e, anchor: t }, n, r) => {
		let i;
		for (; e && e !== t;) i = h(e), o(e, n, r), e = i;
		o(t, n, r);
	}, C = ({ el: e, anchor: t }) => {
		let n;
		for (; e && e !== t;) n = h(e), s(e), e = n;
		s(t);
	}, w = (e, t, n, r, i, a, o, s, c) => {
		if (t.type === "svg" ? o = "svg" : t.type === "math" && (o = "mathml"), e == null) te(t, n, r, i, a, o, s, c);
		else {
			let n = e.el && e.el._isVueCE ? e.el : null;
			try {
				n && n._beginPatch(), re(e, t, i, a, o, s, c);
			} finally {
				n && n._endPatch();
			}
		}
	}, te = (e, t, n, r, i, a, s, u) => {
		let d, f, { props: m, shapeFlag: h, transition: g, dirs: _ } = e;
		if (d = e.el = l(e.type, a, m && m.is, m), h & 8 ? p(d, e.children) : h & 16 && T(e.children, d, null, r, i, Si(e, a), s, u), _ && Tn(e, null, r, "created"), ne(d, e, e.scopeId, s, r), m) {
			for (let e in m) e !== "value" && !ee(e) && c(d, e, null, m[e], a, r);
			"value" in m && c(d, "value", null, m.value, a), (f = m.onVnodeBeforeMount) && $i(f, r, e);
		}
		_ && Tn(e, null, r, "beforeMount");
		let v = wi(i, g);
		v && g.beforeEnter(d), o(d, t, n), ((f = m && m.onVnodeMounted) || v || _) && U(() => {
			try {
				f && $i(f, r, e), v && g.enter(d), _ && Tn(e, null, r, "mounted");
			} finally {}
		}, i);
	}, ne = (e, t, n, r, i) => {
		if (n && g(e, n), r) for (let t = 0; t < r.length; t++) g(e, r[t]);
		if (i) {
			let n = i.subTree;
			if (t === n || Ai(n.type) && (n.ssContent === t || n.ssFallback === t)) {
				let t = i.vnode;
				ne(e, t, t.scopeId, t.slotScopeIds, i.parent);
			}
		}
	}, T = (e, t, n, r, i, a, o, s, c = 0) => {
		for (let l = c; l < e.length; l++) v(null, e[l] = s ? Xi(e[l]) : Yi(e[l]), t, n, r, i, a, o, s);
	}, re = (e, n, r, i, a, o, s) => {
		let l = n.el = e.el, { patchFlag: u, dynamicChildren: d, dirs: f } = n;
		u |= e.patchFlag & 16;
		let m = e.props || t, h = n.props || t, g;
		if (r && Ci(r, !1), (g = h.onVnodeBeforeUpdate) && $i(g, r, n, e), f && Tn(n, e, r, "beforeUpdate"), r && Ci(r, !0), (m.innerHTML && h.innerHTML == null || m.textContent && h.textContent == null) && p(l, ""), d ? E(e.dynamicChildren, d, l, r, i, Si(n, a), o) : s || de(e, n, l, null, r, i, Si(n, a), o, !1), u > 0) {
			if (u & 16) ie(l, m, h, r, a);
			else if (u & 2 && m.class !== h.class && c(l, "class", null, h.class, a), u & 4 && c(l, "style", m.style, h.style, a), u & 8) {
				let e = n.dynamicProps;
				for (let t = 0; t < e.length; t++) {
					let n = e[t], i = m[n], o = h[n];
					(o !== i || n === "value") && c(l, n, i, o, a, r);
				}
			}
			u & 1 && e.children !== n.children && p(l, n.children);
		} else !s && d == null && ie(l, m, h, r, a);
		((g = h.onVnodeUpdated) || f) && U(() => {
			g && $i(g, r, n, e), f && Tn(n, e, r, "updated");
		}, i);
	}, E = (e, t, n, r, i, a, o) => {
		for (let s = 0; s < t.length; s++) {
			let c = e[s], l = t[s];
			v(c, l, c.el && (c.type === W || !Hi(c, l) || c.shapeFlag & 198) ? m(c.el) : n, null, r, i, a, o, !0);
		}
	}, ie = (e, n, r, i, a) => {
		if (n !== r) {
			if (n !== t) for (let t in n) !ee(t) && !(t in r) && c(e, t, n[t], null, a, i);
			for (let t in r) {
				if (ee(t)) continue;
				let o = r[t], s = n[t];
				o !== s && t !== "value" && c(e, t, s, o, a, i);
			}
			"value" in r && c(e, "value", n.value, r.value, a);
		}
	}, ae = (e, t, n, r, i, a, s, c, l) => {
		let d = t.el = e ? e.el : u(""), f = t.anchor = e ? e.anchor : u(""), { patchFlag: p, dynamicChildren: m, slotScopeIds: h } = t;
		h && (c = c ? c.concat(h) : h), e == null ? (o(d, n, r), o(f, n, r), T(t.children || [], n, f, i, a, s, c, l)) : p > 0 && p & 64 && m && e.dynamicChildren && e.dynamicChildren.length === m.length ? (E(e.dynamicChildren, m, n, i, a, s, c), (t.key != null || i && t === i.subTree) && Ti(e, t, !0)) : de(e, t, n, f, i, a, s, c, l);
	}, D = (e, t, n, r, i, a, o, s, c) => {
		t.slotScopeIds = s, e == null ? t.shapeFlag & 512 ? i.ctx.activate(t, n, r, o, c) : O(t, n, r, i, a, o, c) : se(e, t, c);
	}, O = (e, t, n, r, i, a, o) => {
		let s = e.component = na(e, r, i);
		if (nr(e) && (s.ctx.renderer = A), ua(s, !1, o), s.asyncDep) {
			if (i && i.registerDep(s, ce, o), !e.el) {
				let r = s.subTree = Gi(Ni);
				b(null, r, t, n), e.placeholder = r.el;
			}
		} else ce(s, e, t, n, i, a, o);
	}, se = (e, t, n) => {
		let r = t.component = e.component;
		if (Qr(e, t, n)) if (r.asyncDep && !r.asyncResolved) {
			ue(r, t, n);
			return;
		} else r.next = t, r.update();
		else t.el = e.el, r.vnode = t;
	}, ce = (e, t, n, r, i, a, o) => {
		let s = () => {
			if (e.isMounted) {
				let { next: t, bu: n, u: r, parent: s, vnode: c } = e;
				{
					let n = Di(e);
					if (n) {
						t && (t.el = c.el, ue(e, t, o)), n.asyncDep.then(() => {
							U(() => {
								e.isUnmounted || l();
							}, i);
						});
						return;
					}
				}
				let u = t, d;
				Ci(e, !1), t ? (t.el = c.el, ue(e, t, o)) : t = c, n && oe(n), (d = t.props && t.props.onVnodeBeforeUpdate) && $i(d, s, t, c), Ci(e, !0);
				let f = Yr(e), p = e.subTree;
				e.subTree = f, v(p, f, m(p.el), ye(p), e, i, a), t.el = f.el, u === null && ti(e, f.el), r && U(r, i), (d = t.props && t.props.onVnodeUpdated) && U(() => $i(d, s, t, c), i);
			} else {
				let o, { el: s, props: c } = t, { bm: l, m: u, parent: d, root: f, type: p } = e, m = tr(t);
				if (Ci(e, !1), l && oe(l), !m && (o = c && c.onVnodeBeforeMount) && $i(o, d, t), Ci(e, !0), s && Ce) {
					let t = () => {
						e.subTree = Yr(e), Ce(s, e.subTree, e, i, null);
					};
					m && p.__asyncHydrate ? p.__asyncHydrate(s, e, t) : t();
				} else {
					f.ce && f.ce._hasShadowRoot() && f.ce._injectChildStyle(p, e.parent ? e.parent.type : void 0);
					let o = e.subTree = Yr(e);
					v(null, o, n, r, e, i, a), t.el = o.el;
				}
				if (u && U(u, i), !m && (o = c && c.onVnodeMounted)) {
					let e = t;
					U(() => $i(o, d, e), i);
				}
				(t.shapeFlag & 256 || d && tr(d.vnode) && d.vnode.shapeFlag & 256) && e.a && U(e.a, i), e.isMounted = !0, t = n = r = null;
			}
		};
		e.scope.on();
		let c = e.effect = new De(s);
		e.scope.off();
		let l = e.update = c.run.bind(c), u = e.job = c.runIfDirty.bind(c);
		u.i = e, u.id = e.uid, c.scheduler = () => hn(u), Ci(e, !0), l();
	}, ue = (e, t, n) => {
		t.component = e;
		let r = e.vnode.props;
		e.vnode = t, e.next = null, oi(e, t.props, r, n), yi(e, t.children, n), Ve(), vn(e), He();
	}, de = (e, t, n, r, i, a, o, s, c = !1) => {
		let l = e && e.children, u = e ? e.shapeFlag : 0, d = t.children, { patchFlag: f, shapeFlag: m } = t;
		if (f > 0) {
			if (f & 128) {
				pe(l, d, n, r, i, a, o, s, c);
				return;
			} else if (f & 256) {
				fe(l, d, n, r, i, a, o, s, c);
				return;
			}
		}
		m & 8 ? (u & 16 && ve(l, i, a), d !== l && p(n, d)) : u & 16 ? m & 16 ? pe(l, d, n, r, i, a, o, s, c) : ve(l, i, a, !0) : (u & 8 && p(n, ""), m & 16 && T(d, n, r, i, a, o, s, c));
	}, fe = (e, t, r, i, a, o, s, c, l) => {
		e ||= n, t ||= n;
		let u = e.length, d = t.length, f = Math.min(u, d), p;
		for (p = 0; p < f; p++) {
			let n = t[p] = l ? Xi(t[p]) : Yi(t[p]);
			v(e[p], n, r, null, a, o, s, c, l);
		}
		u > d ? ve(e, a, o, !0, !1, f) : T(t, r, i, a, o, s, c, l, f);
	}, pe = (e, t, r, i, a, o, s, c, l) => {
		let u = 0, d = t.length, f = e.length - 1, p = d - 1;
		for (; u <= f && u <= p;) {
			let n = e[u], i = t[u] = l ? Xi(t[u]) : Yi(t[u]);
			if (Hi(n, i)) v(n, i, r, null, a, o, s, c, l);
			else break;
			u++;
		}
		for (; u <= f && u <= p;) {
			let n = e[f], i = t[p] = l ? Xi(t[p]) : Yi(t[p]);
			if (Hi(n, i)) v(n, i, r, null, a, o, s, c, l);
			else break;
			f--, p--;
		}
		if (u > f) {
			if (u <= p) {
				let e = p + 1, n = e < d ? t[e].el : i;
				for (; u <= p;) v(null, t[u] = l ? Xi(t[u]) : Yi(t[u]), r, n, a, o, s, c, l), u++;
			}
		} else if (u > p) for (; u <= f;) k(e[u], a, o, !0), u++;
		else {
			let m = u, h = u, g = /* @__PURE__ */ new Map();
			for (u = h; u <= p; u++) {
				let e = t[u] = l ? Xi(t[u]) : Yi(t[u]);
				e.key != null && g.set(e.key, u);
			}
			let _, y = 0, b = p - h + 1, x = !1, S = 0, C = Array(b);
			for (u = 0; u < b; u++) C[u] = 0;
			for (u = m; u <= f; u++) {
				let n = e[u];
				if (y >= b) {
					k(n, a, o, !0);
					continue;
				}
				let i;
				if (n.key != null) i = g.get(n.key);
				else for (_ = h; _ <= p; _++) if (C[_ - h] === 0 && Hi(n, t[_])) {
					i = _;
					break;
				}
				i === void 0 ? k(n, a, o, !0) : (C[i - h] = u + 1, i >= S ? S = i : x = !0, v(n, t[i], r, null, a, o, s, c, l), y++);
			}
			let w = x ? Ei(C) : n;
			for (_ = w.length - 1, u = b - 1; u >= 0; u--) {
				let e = h + u, n = t[e], f = t[e + 1], p = e + 1 < d ? f.el || ki(f) : i;
				C[u] === 0 ? v(null, n, r, p, a, o, s, c, l) : x && (_ < 0 || u !== w[_] ? me(n, r, p, 2) : _--);
			}
		}
	}, me = (e, t, n, r, i = null) => {
		let { el: a, type: c, transition: l, children: u, shapeFlag: d } = e;
		if (d & 6) {
			me(e.component.subTree, t, n, r);
			return;
		}
		if (d & 128) {
			e.suspense.move(t, n, r);
			return;
		}
		if (d & 64) {
			c.move(e, t, n, A);
			return;
		}
		if (c === W) {
			o(a, t, n);
			for (let e = 0; e < u.length; e++) me(u[e], t, n, r);
			o(e.anchor, t, n);
			return;
		}
		if (c === Pi) {
			S(e, t, n);
			return;
		}
		if (r !== 2 && d & 1 && l) if (r === 0) l.beforeEnter(a), o(a, t, n), U(() => l.enter(a), i);
		else {
			let { leave: r, delayLeave: i, afterLeave: c } = l, u = () => {
				e.ctx.isUnmounted ? s(a) : o(a, t, n);
			}, d = () => {
				a._isLeaving && a[Jn](!0), r(a, () => {
					u(), c && c();
				});
			};
			i ? i(a, u, d) : d();
		}
		else o(a, t, n);
	}, k = (e, t, n, r = !1, i = !1) => {
		let { type: a, props: o, ref: s, children: c, dynamicChildren: l, shapeFlag: u, patchFlag: d, dirs: f, cacheIndex: p, memo: m } = e;
		if (d === -2 && (i = !1), s != null && (Ve(), $n(s, null, n, e, !0), He()), p != null && (t.renderCache[p] = void 0), u & 256) {
			t.ctx.deactivate(e);
			return;
		}
		let h = u & 1 && f, g = !tr(e), _;
		if (g && (_ = o && o.onVnodeBeforeUnmount) && $i(_, t, e), u & 6) _e(e.component, n, r);
		else {
			if (u & 128) {
				e.suspense.unmount(n, r);
				return;
			}
			h && Tn(e, null, t, "beforeUnmount"), u & 64 ? e.type.remove(e, t, n, A, r) : l && !l.hasOnce && (a !== W || d > 0 && d & 64) ? ve(l, t, n, !1, !0) : (a === W && d & 384 || !i && u & 16) && ve(c, t, n), r && he(e);
		}
		let v = m != null && p == null;
		(g && (_ = o && o.onVnodeUnmounted) || h || v) && U(() => {
			_ && $i(_, t, e), h && Tn(e, null, t, "unmounted"), v && (e.el = null);
		}, n);
	}, he = (e) => {
		let { type: t, el: n, anchor: r, transition: i } = e;
		if (t === W) {
			ge(n, r);
			return;
		}
		if (t === Pi) {
			C(e);
			return;
		}
		let a = () => {
			s(n), i && !i.persisted && i.afterLeave && i.afterLeave();
		};
		if (e.shapeFlag & 1 && i && !i.persisted) {
			let { leave: t, delayLeave: r } = i, o = () => t(n, a);
			r ? r(e.el, a, o) : o();
		} else a();
	}, ge = (e, t) => {
		let n;
		for (; e !== t;) n = h(e), s(e), e = n;
		s(t);
	}, _e = (e, t, n) => {
		let { bum: r, scope: i, job: a, subTree: o, um: s, m: c, a: l } = e;
		Oi(c), Oi(l), r && oe(r), i.stop(), a && (a.flags |= 8, k(o, e, t, n)), s && U(s, t), U(() => {
			e.isUnmounted = !0;
		}, t);
	}, ve = (e, t, n, r = !1, i = !1, a = 0) => {
		for (let o = a; o < e.length; o++) k(e[o], t, n, r, i);
	}, ye = (e) => {
		if (e.shapeFlag & 6) return ye(e.component.subTree);
		if (e.shapeFlag & 128) return e.suspense.next();
		let t = h(e.anchor || e.el), n = t && t[Fn];
		return n ? h(n) : t;
	}, be = !1, xe = (e, t, n) => {
		let r;
		e == null ? t._vnode && (k(t._vnode, null, null, !0), r = t._vnode.component) : v(t._vnode || null, e, t, null, null, null, n), t._vnode = e, be ||= (be = !0, vn(r), yn(), !1);
	}, A = {
		p: v,
		um: k,
		m: me,
		r: he,
		mt: O,
		mc: T,
		pc: de,
		pbc: E,
		n: ye,
		o: e
	}, Se, Ce;
	return i && ([Se, Ce] = i(A)), {
		render: xe,
		hydrate: Se,
		createApp: Hr(xe, Se)
	};
}
function Si({ type: e, props: t }, n) {
	return n === "svg" && e === "foreignObject" || n === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : n;
}
function Ci({ effect: e, job: t }, n) {
	n ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function wi(e, t) {
	return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function Ti(e, t, n = !1) {
	let r = e.children, i = t.children;
	if (d(r) && d(i)) for (let e = 0; e < r.length; e++) {
		let t = r[e], a = i[e];
		a.shapeFlag & 1 && !a.dynamicChildren && ((a.patchFlag <= 0 || a.patchFlag === 32) && (a = i[e] = Xi(i[e]), a.el = t.el), !n && a.patchFlag !== -2 && Ti(t, a)), a.type === Mi && (a.patchFlag === -1 && (a = i[e] = Xi(a)), a.el = t.el), a.type === Ni && !a.el && (a.el = t.el);
	}
}
function Ei(e) {
	let t = e.slice(), n = [0], r, i, a, o, s, c = e.length;
	for (r = 0; r < c; r++) {
		let c = e[r];
		if (c !== 0) {
			if (i = n[n.length - 1], e[i] < c) {
				t[r] = i, n.push(r);
				continue;
			}
			for (a = 0, o = n.length - 1; a < o;) s = a + o >> 1, e[n[s]] < c ? a = s + 1 : o = s;
			c < e[n[a]] && (a > 0 && (t[r] = n[a - 1]), n[a] = r);
		}
	}
	for (a = n.length, o = n[a - 1]; a-- > 0;) n[a] = o, o = t[o];
	return n;
}
function Di(e) {
	let t = e.subTree.component;
	if (t) return t.asyncDep && !t.asyncResolved ? t : Di(t);
}
function Oi(e) {
	if (e) for (let t = 0; t < e.length; t++) e[t].flags |= 8;
}
function ki(e) {
	if (e.placeholder) return e.placeholder;
	let t = e.component;
	return t ? ki(t.subTree) : null;
}
var Ai = (e) => e.__isSuspense;
function ji(e, t) {
	t && t.pendingBranch ? d(e) ? t.effects.push(...e) : t.effects.push(e) : _n(e);
}
var W = /* @__PURE__ */ Symbol.for("v-fgt"), Mi = /* @__PURE__ */ Symbol.for("v-txt"), Ni = /* @__PURE__ */ Symbol.for("v-cmt"), Pi = /* @__PURE__ */ Symbol.for("v-stc"), Fi = [], G = null;
function K(e = !1) {
	Fi.push(G = e ? null : []);
}
function Ii() {
	Fi.pop(), G = Fi[Fi.length - 1] || null;
}
var Li = 1;
function Ri(e, t = !1) {
	Li += e, e < 0 && G && t && (G.hasOnce = !0);
}
function zi(e) {
	return e.dynamicChildren = Li > 0 ? G || n : null, Ii(), Li > 0 && G && G.push(e), e;
}
function q(e, t, n, r, i, a) {
	return zi(J(e, t, n, r, i, a, !0));
}
function Bi(e, t, n, r, i) {
	return zi(Gi(e, t, n, r, i, !0));
}
function Vi(e) {
	return e ? e.__v_isVNode === !0 : !1;
}
function Hi(e, t) {
	return e.type === t.type && e.key === t.key;
}
var Ui = ({ key: e }) => e ?? null, Wi = ({ ref: e, ref_key: t, ref_for: n }) => (typeof e == "number" && (e = "" + e), e == null ? null : g(e) || /* @__PURE__ */ L(e) || h(e) ? {
	i: B,
	r: e,
	k: t,
	f: !!n
} : e);
function J(e, t = null, n = null, r = 0, i = null, a = e === W ? 0 : 1, o = !1, s = !1) {
	let c = {
		__v_isVNode: !0,
		__v_skip: !0,
		type: e,
		props: t,
		key: t && Ui(t),
		ref: t && Wi(t),
		scopeId: Sn,
		slotScopeIds: null,
		children: n,
		component: null,
		suspense: null,
		ssContent: null,
		ssFallback: null,
		dirs: null,
		transition: null,
		el: null,
		anchor: null,
		target: null,
		targetStart: null,
		targetAnchor: null,
		staticCount: 0,
		shapeFlag: a,
		patchFlag: r,
		dynamicProps: i,
		dynamicChildren: null,
		appContext: null,
		ctx: B
	};
	return s ? (Zi(c, n), a & 128 && e.normalize(c)) : n && (c.shapeFlag |= g(n) ? 8 : 16), Li > 0 && !o && G && (c.patchFlag > 0 || a & 6) && c.patchFlag !== 32 && G.push(c), c;
}
var Gi = Ki;
function Ki(e, t = null, n = null, r = 0, i = null, a = !1) {
	if ((!e || e === yr) && (e = Ni), Vi(e)) {
		let r = Ji(e, t, !0);
		return n && Zi(r, n), Li > 0 && !a && G && (r.shapeFlag & 6 ? G[G.indexOf(e)] = r : G.push(r)), r.patchFlag = -2, r;
	}
	if (ya(e) && (e = e.__vccOpts), t) {
		t = qi(t);
		let { class: e, style: n } = t;
		e && !g(e) && (t.class = k(e)), v(n) && (/* @__PURE__ */ zt(n) && !d(n) && (n = s({}, n)), t.style = ue(n));
	}
	let o = g(e) ? 1 : Ai(e) ? 128 : In(e) ? 64 : v(e) ? 4 : h(e) ? 2 : 0;
	return J(e, t, n, r, i, o, a, !0);
}
function qi(e) {
	return e ? /* @__PURE__ */ zt(e) || ii(e) ? s({}, e) : e : null;
}
function Ji(e, t, n = !1, r = !1) {
	let { props: i, ref: a, patchFlag: o, children: s, transition: c } = e, l = t ? Qi(i || {}, t) : i, u = {
		__v_isVNode: !0,
		__v_skip: !0,
		type: e.type,
		props: l,
		key: l && Ui(l),
		ref: t && t.ref ? n && a ? d(a) ? a.concat(Wi(t)) : [a, Wi(t)] : Wi(t) : a,
		scopeId: e.scopeId,
		slotScopeIds: e.slotScopeIds,
		children: s,
		target: e.target,
		targetStart: e.targetStart,
		targetAnchor: e.targetAnchor,
		staticCount: e.staticCount,
		shapeFlag: e.shapeFlag,
		patchFlag: t && e.type !== W ? o === -1 ? 16 : o | 16 : o,
		dynamicProps: e.dynamicProps,
		dynamicChildren: e.dynamicChildren,
		appContext: e.appContext,
		dirs: e.dirs,
		transition: c,
		component: e.component,
		suspense: e.suspense,
		ssContent: e.ssContent && Ji(e.ssContent),
		ssFallback: e.ssFallback && Ji(e.ssFallback),
		placeholder: e.placeholder,
		el: e.el,
		anchor: e.anchor,
		ctx: e.ctx,
		ce: e.ce
	};
	return c && r && Yn(u, c.clone(u)), u;
}
function Y(e = " ", t = 0) {
	return Gi(Mi, null, e, t);
}
function X(e = "", t = !1) {
	return t ? (K(), Bi(Ni, null, e)) : Gi(Ni, null, e);
}
function Yi(e) {
	return e == null || typeof e == "boolean" ? Gi(Ni) : d(e) ? Gi(W, null, e.slice()) : Vi(e) ? Xi(e) : Gi(Mi, null, String(e));
}
function Xi(e) {
	return e.el === null && e.patchFlag !== -1 || e.memo ? e : Ji(e);
}
function Zi(e, t) {
	let n = 0, { shapeFlag: r } = e;
	if (t == null) t = null;
	else if (d(t)) n = 16;
	else if (typeof t == "object") if (r & 65) {
		let n = t.default;
		n && (n._c && (n._d = !1), Zi(e, n()), n._c && (n._d = !0));
		return;
	} else {
		n = 32;
		let r = t._;
		!r && !ii(t) ? t._ctx = B : r === 3 && B && (B.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
	}
	else h(t) ? (t = {
		default: t,
		_ctx: B
	}, n = 32) : (t = String(t), r & 64 ? (n = 16, t = [Y(t)]) : n = 8);
	e.children = t, e.shapeFlag |= n;
}
function Qi(...e) {
	let t = {};
	for (let n = 0; n < e.length; n++) {
		let r = e[n];
		for (let e in r) if (e === "class") t.class !== r.class && (t.class = k([t.class, r.class]));
		else if (e === "style") t.style = ue([t.style, r.style]);
		else if (a(e)) {
			let n = t[e], i = r[e];
			i && n !== i && !(d(n) && n.includes(i)) ? t[e] = n ? [].concat(n, i) : i : i == null && n == null && !o(e) && (t[e] = i);
		} else e !== "" && (t[e] = r[e]);
	}
	return t;
}
function $i(e, t, n, r = null) {
	rn(e, t, 7, [n, r]);
}
var ea = Br(), ta = 0;
function na(e, n, r) {
	let i = e.type, a = (n ? n.appContext : e.appContext) || ea, o = {
		uid: ta++,
		vnode: e,
		type: i,
		parent: n,
		appContext: a,
		root: null,
		next: null,
		subTree: null,
		effect: null,
		update: null,
		job: null,
		scope: new we(!0),
		render: null,
		proxy: null,
		exposed: null,
		exposeProxy: null,
		withProxy: null,
		provides: n ? n.provides : Object.create(a.provides),
		ids: n ? n.ids : [
			"",
			0,
			0
		],
		accessCache: null,
		renderCache: [],
		components: null,
		directives: null,
		propsOptions: ui(i, a),
		emitsOptions: qr(i, a),
		emit: null,
		emitted: null,
		propsDefaults: t,
		inheritAttrs: i.inheritAttrs,
		ctx: t,
		data: t,
		props: t,
		attrs: t,
		slots: t,
		refs: t,
		setupState: t,
		setupContext: null,
		suspense: r,
		suspenseId: r ? r.pendingId : 0,
		asyncDep: null,
		asyncResolved: !1,
		isMounted: !1,
		isUnmounted: !1,
		isDeactivated: !1,
		bc: null,
		c: null,
		bm: null,
		m: null,
		bu: null,
		u: null,
		um: null,
		bum: null,
		da: null,
		a: null,
		rtg: null,
		rtc: null,
		ec: null,
		sp: null
	};
	return o.ctx = { _: o }, o.root = n ? n.root : o, o.emit = Gr.bind(null, o), e.ce && e.ce(o), o;
}
var Z = null, ra = () => Z || B, ia, aa;
{
	let e = le(), t = (t, n) => {
		let r;
		return (r = e[t]) || (r = e[t] = []), r.push(n), (e) => {
			r.length > 1 ? r.forEach((t) => t(e)) : r[0](e);
		};
	};
	ia = t("__VUE_INSTANCE_SETTERS__", (e) => Z = e), aa = t("__VUE_SSR_SETTERS__", (e) => la = e);
}
var oa = (e) => {
	let t = Z;
	return ia(e), e.scope.on(), () => {
		e.scope.off(), ia(t);
	};
}, sa = () => {
	Z && Z.scope.off(), ia(null);
};
function ca(e) {
	return e.vnode.shapeFlag & 4;
}
var la = !1;
function ua(e, t = !1, n = !1) {
	t && aa(t);
	let { props: r, children: i } = e.vnode, a = ca(e);
	ai(e, r, a, t), vi(e, i, n || t);
	let o = a ? da(e, t) : void 0;
	return t && aa(!1), o;
}
function da(e, t) {
	let n = e.type;
	e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, wr);
	let { setup: r } = n;
	if (r) {
		Ve();
		let n = e.setupContext = r.length > 1 ? _a(e) : null, i = oa(e), a = nn(r, e, 0, [e.props, n]), o = y(a);
		if (He(), i(), (o || e.sp) && !tr(e) && Xn(e), o) {
			if (a.then(sa, sa), t) return a.then((n) => {
				fa(e, n, t);
			}).catch((t) => {
				an(t, e, 0);
			});
			e.asyncDep = a;
		} else fa(e, a, t);
	} else ha(e, t);
}
function fa(e, t, n) {
	h(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : v(t) && (e.setupState = qt(t)), ha(e, n);
}
var pa, ma;
function ha(e, t, n) {
	let i = e.type;
	if (!e.render) {
		if (!t && pa && !i.render) {
			let t = i.template || jr(e).template;
			if (t) {
				let { isCustomElement: n, compilerOptions: r } = e.appContext.config, { delimiters: a, compilerOptions: o } = i;
				i.render = pa(t, s(s({
					isCustomElement: n,
					delimiters: a
				}, r), o));
			}
		}
		e.render = i.render || r, ma && ma(e);
	}
	{
		let t = oa(e);
		Ve();
		try {
			Dr(e);
		} finally {
			He(), t();
		}
	}
}
var ga = { get(e, t) {
	return P(e, "get", ""), e[t];
} };
function _a(e) {
	return {
		attrs: new Proxy(e.attrs, ga),
		slots: e.slots,
		emit: e.emit,
		expose: (t) => {
			e.exposed = t || {};
		}
	};
}
function va(e) {
	return e.exposed ? e.exposeProxy ||= new Proxy(qt(Bt(e.exposed)), {
		get(t, n) {
			if (n in t) return t[n];
			if (n in Sr) return Sr[n](e);
		},
		has(e, t) {
			return t in e || t in Sr;
		}
	}) : e.proxy;
}
function ya(e) {
	return h(e) && "__vccOpts" in e;
}
var ba = (e, t) => /* @__PURE__ */ Yt(e, t, la), xa = "3.5.34", Sa = void 0, Ca = typeof window < "u" && window.trustedTypes;
if (Ca) try {
	Sa = /* @__PURE__ */ Ca.createPolicy("vue", { createHTML: (e) => e });
} catch {}
var wa = Sa ? (e) => Sa.createHTML(e) : (e) => e, Ta = "http://www.w3.org/2000/svg", Ea = "http://www.w3.org/1998/Math/MathML", Da = typeof document < "u" ? document : null, Oa = Da && /* @__PURE__ */ Da.createElement("template"), ka = {
	insert: (e, t, n) => {
		t.insertBefore(e, n || null);
	},
	remove: (e) => {
		let t = e.parentNode;
		t && t.removeChild(e);
	},
	createElement: (e, t, n, r) => {
		let i = t === "svg" ? Da.createElementNS(Ta, e) : t === "mathml" ? Da.createElementNS(Ea, e) : n ? Da.createElement(e, { is: n }) : Da.createElement(e);
		return e === "select" && r && r.multiple != null && i.setAttribute("multiple", r.multiple), i;
	},
	createText: (e) => Da.createTextNode(e),
	createComment: (e) => Da.createComment(e),
	setText: (e, t) => {
		e.nodeValue = t;
	},
	setElementText: (e, t) => {
		e.textContent = t;
	},
	parentNode: (e) => e.parentNode,
	nextSibling: (e) => e.nextSibling,
	querySelector: (e) => Da.querySelector(e),
	setScopeId(e, t) {
		e.setAttribute(t, "");
	},
	insertStaticContent(e, t, n, r, i, a) {
		let o = n ? n.previousSibling : t.lastChild;
		if (i && (i === a || i.nextSibling)) for (; t.insertBefore(i.cloneNode(!0), n), !(i === a || !(i = i.nextSibling)););
		else {
			Oa.innerHTML = wa(r === "svg" ? `<svg>${e}</svg>` : r === "mathml" ? `<math>${e}</math>` : e);
			let i = Oa.content;
			if (r === "svg" || r === "mathml") {
				let e = i.firstChild;
				for (; e.firstChild;) i.appendChild(e.firstChild);
				i.removeChild(e);
			}
			t.insertBefore(i, n);
		}
		return [o ? o.nextSibling : t.firstChild, n ? n.previousSibling : t.lastChild];
	}
}, Aa = /* @__PURE__ */ Symbol("_vtc");
function ja(e, t, n) {
	let r = e[Aa];
	r && (t = (t ? [t, ...r] : [...r]).join(" ")), t == null ? e.removeAttribute("class") : n ? e.setAttribute("class", t) : e.className = t;
}
var Ma = /* @__PURE__ */ Symbol("_vod"), Na = /* @__PURE__ */ Symbol("_vsh"), Pa = /* @__PURE__ */ Symbol(""), Fa = /(?:^|;)\s*display\s*:/;
function Ia(e, t, n) {
	let r = e.style, i = g(n), a = !1;
	if (n && !i) {
		if (t) if (g(t)) for (let e of t.split(";")) {
			let t = e.slice(0, e.indexOf(":")).trim();
			n[t] ?? Ra(r, t, "");
		}
		else for (let e in t) n[e] ?? Ra(r, e, "");
		for (let i in n) {
			i === "display" && (a = !0);
			let o = n[i];
			o == null ? Ra(r, i, "") : Ha(e, i, !g(t) && t ? t[i] : void 0, o) || Ra(r, i, o);
		}
	} else if (i) {
		if (t !== n) {
			let e = r[Pa];
			e && (n += ";" + e), r.cssText = n, a = Fa.test(n);
		}
	} else t && e.removeAttribute("style");
	Ma in e && (e[Ma] = a ? r.display : "", e[Na] && (r.display = "none"));
}
var La = /\s*!important$/;
function Ra(e, t, n) {
	if (d(n)) n.forEach((n) => Ra(e, t, n));
	else if (n ??= "", t.startsWith("--")) e.setProperty(t, n);
	else {
		let r = Va(e, t);
		La.test(n) ? e.setProperty(E(r), n.replace(La, ""), "important") : e[r] = n;
	}
}
var za = [
	"Webkit",
	"Moz",
	"ms"
], Ba = {};
function Va(e, t) {
	let n = Ba[t];
	if (n) return n;
	let r = T(t);
	if (r !== "filter" && r in e) return Ba[t] = r;
	r = ie(r);
	for (let n = 0; n < za.length; n++) {
		let i = za[n] + r;
		if (i in e) return Ba[t] = i;
	}
	return t;
}
function Ha(e, t, n, r) {
	return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && g(r) && n === r;
}
var Ua = "http://www.w3.org/1999/xlink";
function Wa(e, t, n, r, i, a = ge(t)) {
	r && t.startsWith("xlink:") ? n == null ? e.removeAttributeNS(Ua, t.slice(6, t.length)) : e.setAttributeNS(Ua, t, n) : n == null || a && !_e(n) ? e.removeAttribute(t) : e.setAttribute(t, a ? "" : _(n) ? String(n) : n);
}
function Ga(e, t, n, r, i) {
	if (t === "innerHTML" || t === "textContent") {
		n != null && (e[t] = t === "innerHTML" ? wa(n) : n);
		return;
	}
	let a = e.tagName;
	if (t === "value" && a !== "PROGRESS" && !a.includes("-")) {
		let r = a === "OPTION" ? e.getAttribute("value") || "" : e.value, i = n == null ? e.type === "checkbox" ? "on" : "" : String(n);
		(r !== i || !("_value" in e)) && (e.value = i), n ?? e.removeAttribute(t), e._value = n;
		return;
	}
	let o = !1;
	if (n === "" || n == null) {
		let r = typeof e[t];
		r === "boolean" ? n = _e(n) : n == null && r === "string" ? (n = "", o = !0) : r === "number" && (n = 0, o = !0);
	}
	try {
		e[t] = n;
	} catch {}
	o && e.removeAttribute(i || t);
}
function Ka(e, t, n, r) {
	e.addEventListener(t, n, r);
}
function qa(e, t, n, r) {
	e.removeEventListener(t, n, r);
}
var Ja = /* @__PURE__ */ Symbol("_vei");
function Ya(e, t, n, r, i = null) {
	let a = e[Ja] || (e[Ja] = {}), o = a[t];
	if (r && o) o.value = r;
	else {
		let [n, s] = Za(t);
		r ? Ka(e, n, a[t] = to(r, i), s) : o && (qa(e, n, o, s), a[t] = void 0);
	}
}
var Xa = /(?:Once|Passive|Capture)$/;
function Za(e) {
	let t;
	if (Xa.test(e)) {
		t = {};
		let n;
		for (; n = e.match(Xa);) e = e.slice(0, e.length - n[0].length), t[n[0].toLowerCase()] = !0;
	}
	return [e[2] === ":" ? e.slice(3) : E(e.slice(2)), t];
}
var Qa = 0, $a = /* @__PURE__ */ Promise.resolve(), eo = () => Qa ||= ($a.then(() => Qa = 0), Date.now());
function to(e, t) {
	let n = (e) => {
		if (!e._vts) e._vts = Date.now();
		else if (e._vts <= n.attached) return;
		rn(no(e, n.value), t, 5, [e]);
	};
	return n.value = e, n.attached = eo(), n;
}
function no(e, t) {
	if (d(t)) {
		let n = e.stopImmediatePropagation;
		return e.stopImmediatePropagation = () => {
			n.call(e), e._stopped = !0;
		}, t.map((e) => (t) => !t._stopped && e && e(t));
	} else return t;
}
var ro = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, io = (e, t, n, r, i, s) => {
	let c = i === "svg";
	t === "class" ? ja(e, r, c) : t === "style" ? Ia(e, n, r) : a(t) ? o(t) || Ya(e, t, n, r, s) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : ao(e, t, r, c)) ? (Ga(e, t, r), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && Wa(e, t, r, c, s, t !== "value")) : e._isVueCE && (oo(e, t) || e._def.__asyncLoader && (/[A-Z]/.test(t) || !g(r))) ? Ga(e, T(t), r, s, t) : (t === "true-value" ? e._trueValue = r : t === "false-value" && (e._falseValue = r), Wa(e, t, r, c));
};
function ao(e, t, n, r) {
	if (r) return !!(t === "innerHTML" || t === "textContent" || t in e && ro(t) && h(n));
	if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA") return !1;
	if (t === "width" || t === "height") {
		let t = e.tagName;
		if (t === "IMG" || t === "VIDEO" || t === "CANVAS" || t === "SOURCE") return !1;
	}
	return ro(t) && g(n) ? !1 : t in e;
}
function oo(e, t) {
	let n = e._def.props;
	if (!n) return !1;
	let r = T(t);
	return Array.isArray(n) ? n.some((e) => T(e) === r) : Object.keys(n).some((e) => T(e) === r);
}
var so = (e) => {
	let t = e.props["onUpdate:modelValue"] || !1;
	return d(t) ? (e) => oe(t, e) : t;
};
function co(e) {
	e.target.composing = !0;
}
function lo(e) {
	let t = e.target;
	t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
var uo = /* @__PURE__ */ Symbol("_assign");
function fo(e, t, n) {
	return t && (e = e.trim()), n && (e = se(e)), e;
}
var Q = {
	created(e, { modifiers: { lazy: t, trim: n, number: r } }, i) {
		e[uo] = so(i);
		let a = r || i.props && i.props.type === "number";
		Ka(e, t ? "change" : "input", (t) => {
			t.target.composing || e[uo](fo(e.value, n, a));
		}), (n || a) && Ka(e, "change", () => {
			e.value = fo(e.value, n, a);
		}), t || (Ka(e, "compositionstart", co), Ka(e, "compositionend", lo), Ka(e, "change", lo));
	},
	mounted(e, { value: t }) {
		e.value = t ?? "";
	},
	beforeUpdate(e, { value: t, oldValue: n, modifiers: { lazy: r, trim: i, number: a } }, o) {
		if (e[uo] = so(o), e.composing) return;
		let s = (a || e.type === "number") && !/^0\d/.test(e.value) ? se(e.value) : e.value, c = t ?? "";
		if (s === c) return;
		let l = e.getRootNode();
		(l instanceof Document || l instanceof ShadowRoot) && l.activeElement === e && e.type !== "range" && (r && t === n || i && e.value.trim() === c) || (e.value = c);
	}
}, $ = {
	deep: !0,
	created(e, t, n) {
		e[uo] = so(n), Ka(e, "change", () => {
			let t = e._modelValue, n = go(e), r = e.checked, i = e[uo];
			if (d(t)) {
				let e = be(t, n), a = e !== -1;
				if (r && !a) i(t.concat(n));
				else if (!r && a) {
					let n = [...t];
					n.splice(e, 1), i(n);
				}
			} else if (p(t)) {
				let e = new Set(t);
				r ? e.add(n) : e.delete(n), i(e);
			} else i(_o(e, r));
		});
	},
	mounted: po,
	beforeUpdate(e, t, n) {
		e[uo] = so(n), po(e, t, n);
	}
};
function po(e, { value: t, oldValue: n }, r) {
	e._modelValue = t;
	let i;
	if (d(t)) i = be(t, r.props.value) > -1;
	else if (p(t)) i = t.has(r.props.value);
	else {
		if (t === n) return;
		i = ye(t, _o(e, !0));
	}
	e.checked !== i && (e.checked = i);
}
var mo = {
	deep: !0,
	created(e, { value: t, modifiers: { number: n } }, r) {
		let i = p(t);
		Ka(e, "change", () => {
			let t = Array.prototype.filter.call(e.options, (e) => e.selected).map((e) => n ? se(go(e)) : go(e));
			e[uo](e.multiple ? i ? new Set(t) : t : t[0]), e._assigning = !0, pn(() => {
				e._assigning = !1;
			});
		}), e[uo] = so(r);
	},
	mounted(e, { value: t }) {
		ho(e, t);
	},
	beforeUpdate(e, t, n) {
		e[uo] = so(n);
	},
	updated(e, { value: t }) {
		e._assigning || ho(e, t);
	}
};
function ho(e, t) {
	let n = e.multiple, r = d(t);
	if (!(n && !r && !p(t))) {
		for (let i = 0, a = e.options.length; i < a; i++) {
			let a = e.options[i], o = go(a);
			if (n) if (r) {
				let e = typeof o;
				e === "string" || e === "number" ? a.selected = t.some((e) => String(e) === String(o)) : a.selected = be(t, o) > -1;
			} else a.selected = t.has(o);
			else if (ye(go(a), t)) {
				e.selectedIndex !== i && (e.selectedIndex = i);
				return;
			}
		}
		!n && e.selectedIndex !== -1 && (e.selectedIndex = -1);
	}
}
function go(e) {
	return "_value" in e ? e._value : e.value;
}
function _o(e, t) {
	let n = t ? "_trueValue" : "_falseValue";
	return n in e ? e[n] : t;
}
var vo = [
	"ctrl",
	"shift",
	"alt",
	"meta"
], yo = {
	stop: (e) => e.stopPropagation(),
	prevent: (e) => e.preventDefault(),
	self: (e) => e.target !== e.currentTarget,
	ctrl: (e) => !e.ctrlKey,
	shift: (e) => !e.shiftKey,
	alt: (e) => !e.altKey,
	meta: (e) => !e.metaKey,
	left: (e) => "button" in e && e.button !== 0,
	middle: (e) => "button" in e && e.button !== 1,
	right: (e) => "button" in e && e.button !== 2,
	exact: (e, t) => vo.some((n) => e[`${n}Key`] && !t.includes(n))
}, bo = (e, t) => {
	if (!e) return e;
	let n = e._withMods ||= {}, r = t.join(".");
	return n[r] || (n[r] = ((n, ...r) => {
		for (let e = 0; e < t.length; e++) {
			let r = yo[t[e]];
			if (r && r(n, t)) return;
		}
		return e(n, ...r);
	}));
}, xo = /* @__PURE__ */ s({ patchProp: io }, ka), So;
function Co() {
	return So ||= bi(xo);
}
var wo = ((...e) => {
	let t = Co().createApp(...e), { mount: n } = t;
	return t.mount = (e) => {
		let r = Eo(e);
		if (!r) return;
		let i = t._component;
		!h(i) && !i.render && !i.template && (i.template = r.innerHTML), r.nodeType === 1 && (r.textContent = "");
		let a = n(r, !1, To(r));
		return r instanceof Element && (r.removeAttribute("v-cloak"), r.setAttribute("data-v-app", "")), a;
	}, t;
});
function To(e) {
	if (e instanceof SVGElement) return "svg";
	if (typeof MathMLElement == "function" && e instanceof MathMLElement) return "mathml";
}
function Eo(e) {
	return g(e) ? document.querySelector(e) : e;
}
//#endregion
//#region src/utils/SubscriptionFactory.js
var Do = { createDefault: (e = {}) => ({
	Name: "",
	IsEnabled: !0,
	Search: {
		Criteria: [{
			Fields: ["Title", "Topic"],
			Query: "",
			IsExclude: !1
		}],
		MinDurationMinutes: e.SearchSettings?.MinDurationMinutes || null,
		MaxDurationMinutes: e.SearchSettings?.MaxDurationMinutes || null,
		MinBroadcastDate: null,
		MaxBroadcastDate: null
	},
	Download: {
		DownloadPath: "",
		UseStreamingUrlFiles: e.DownloadSettings?.UseStreamingUrlFiles || !1,
		AlwaysCreateSubfolder: e.DownloadSettings?.AlwaysCreateSubfolder || !1,
		AllowFallbackToLowerQuality: e.DownloadSettings?.AllowFallbackToLowerQuality ?? !0,
		EnhancedDuplicateDetection: e.DownloadSettings?.EnhancedDuplicateDetection || !1,
		QualityCheckWithUrl: e.DownloadSettings?.QualityCheckWithUrl || !1,
		DownloadFullVideoForSecondaryAudio: e.DownloadSettings?.DownloadFullVideoForSecondaryAudio || !1
	},
	Series: {
		EnforceSeriesParsing: e.SeriesSettings?.EnforceSeriesParsing || !1,
		AllowAbsoluteEpisodeNumbering: e.SeriesSettings?.AllowAbsoluteEpisodeNumbering || !1,
		TreatNonEpisodesAsExtras: e.SeriesSettings?.TreatNonEpisodesAsExtras || !1,
		SaveTrailers: e.SeriesSettings?.SaveTrailers ?? !0,
		SaveInterviews: e.SeriesSettings?.SaveInterviews ?? !0,
		SaveGenericExtras: e.SeriesSettings?.SaveGenericExtras ?? !0,
		SaveExtrasAsStrm: e.SeriesSettings?.SaveExtrasAsStrm || !1
	},
	Metadata: {
		OriginalLanguage: e.MetadataSettings?.OriginalLanguage || "",
		CreateNfo: e.MetadataSettings?.CreateNfo || !1,
		AppendDateToTitle: e.MetadataSettings?.AppendDateToTitle || !1,
		KeepOriginalTitle: e.MetadataSettings?.KeepOriginalTitle || !1,
		AppendTimeToTitle: e.MetadataSettings?.AppendTimeToTitle || !1
	},
	Accessibility: {
		AllowAudioDescription: e.AccessibilitySettings?.AllowAudioDescription || !1,
		AllowSignLanguage: e.AccessibilitySettings?.AllowSignLanguage || !1
	}
}) }, Oo = (e, t) => {
	let n = e.__vccOpts || e;
	for (let [e, r] of t) n[e] = r;
	return n;
}, ko = { class: "card" }, Ao = { class: "search-grid" }, jo = { class: "field" }, Mo = { class: "field" }, No = { class: "field" }, Po = { class: "field" }, Fo = { class: "field" }, Io = { class: "field" }, Lo = { class: "field" }, Ro = { class: "field" }, zo = { class: "form-actions" }, Bo = ["disabled"], Vo = {
	key: 0,
	class: "results-list"
}, Ho = { class: "result-info" }, Uo = { class: "result-title" }, Wo = { class: "result-meta" }, Go = {
	key: 0,
	class: "material-icons closed_caption",
	title: "Untertitel verfügbar"
}, Ko = { class: "result-meta" }, qo = { class: "result-actions" }, Jo = ["onClick"], Yo = ["onClick"], Xo = {
	key: 1,
	class: "no-results"
}, Zo = /* @__PURE__ */ Oo({
	__name: "SearchTab",
	props: { onCreateSub: {
		type: Function,
		required: !0
	} },
	setup(e) {
		let t = e, n = window.ApiClient ?? null, r = /* @__PURE__ */ R(""), i = /* @__PURE__ */ R(""), a = /* @__PURE__ */ R(""), o = /* @__PURE__ */ R(""), s = /* @__PURE__ */ R(null), c = /* @__PURE__ */ R(null), l = /* @__PURE__ */ R(null), u = /* @__PURE__ */ R(null), d = /* @__PURE__ */ R([]), f = /* @__PURE__ */ R(!1), p = null;
		async function m() {
			if (n) {
				if (!r.value && !i.value && !a.value && !o.value) {
					d.value = [];
					return;
				}
				f.value = !0;
				try {
					let e = new URLSearchParams();
					if (r.value && e.append("title", r.value), i.value && e.append("topic", i.value), a.value && e.append("channel", a.value), o.value && e.append("combinedSearch", o.value), s.value && e.append("minDuration", s.value * 60), c.value && e.append("maxDuration", c.value * 60), l.value && e.append("minBroadcastDate", new Date(l.value).toISOString()), u.value) {
						let t = new Date(u.value);
						t.setHours(23, 59, 59, 999), e.append("maxBroadcastDate", t.toISOString());
					}
					let t = n.getUrl("MediathekViewDL/Search?" + e.toString());
					d.value = await n.getJSON(t);
				} catch (e) {
					console.error("Search failed", e);
				} finally {
					f.value = !1;
				}
			}
		}
		function h() {
			clearTimeout(p), p = setTimeout(() => {
				m();
			}, 500);
		}
		An([
			r,
			i,
			a,
			o,
			s,
			c,
			l,
			u
		], () => {
			if (!r.value && !i.value && !a.value && !o.value) {
				d.value = [];
				return;
			}
			h();
		});
		function g(e) {
			let t = [...e.VideoUrls].sort((e, t) => (t.Quality || 0) - (e.Quality || 0))[0];
			t?.Url && window.open(t.Url, "_blank");
		}
		async function _() {
			if (n) try {
				let e = new URLSearchParams();
				r.value && e.append("title", r.value), i.value && e.append("topic", i.value), a.value && e.append("channel", a.value), o.value && e.append("combinedSearch", o.value);
				let d = n.getUrl("MediathekViewDL/Search/Criteria?" + e.toString()), f = await n.getJSON(d), p = Do.createDefault();
				p.Name = r.value || i.value || o.value || "Suche", p.Search.Criteria = f, p.Search.MinDurationMinutes = s.value, p.Search.MaxDurationMinutes = c.value, p.Search.MinBroadcastDate = l.value ? new Date(l.value).toISOString() : null, p.Search.MaxBroadcastDate = u.value ? new Date(u.value).toISOString() : null, t.onCreateSub(p);
			} catch (e) {
				console.error("Failed to convert criteria", e);
			}
		}
		async function v(e) {
			if (n) try {
				let r = new URLSearchParams();
				e.Title && r.append("title", e.Title), e.Topic && r.append("topic", e.Topic), e.Channel && r.append("channel", e.Channel);
				let i = n.getUrl("MediathekViewDL/Search/Criteria?" + r.toString()), a = await n.getJSON(i), o = Do.createDefault();
				o.Name = e.Topic || e.Title, o.Search.Criteria = a, t.onCreateSub(o);
			} catch (e) {
				console.error("Failed to convert item criteria", e);
			}
		}
		return (e, t) => (K(), q("div", ko, [
			t[17] ||= J("h2", null, "Suche", -1),
			J("form", {
				onSubmit: bo(m, ["prevent"]),
				class: "search-form"
			}, [J("div", Ao, [
				J("div", jo, [t[8] ||= J("label", null, "Titel", -1), V(J("input", {
					"onUpdate:modelValue": t[0] ||= (e) => r.value = e,
					type: "text",
					class: "field-input",
					placeholder: "Titel der Sendung"
				}, null, 512), [[Q, r.value]])]),
				J("div", Mo, [t[9] ||= J("label", null, "Thema", -1), V(J("input", {
					"onUpdate:modelValue": t[1] ||= (e) => i.value = e,
					type: "text",
					class: "field-input",
					placeholder: "Thema / Sendereihe"
				}, null, 512), [[Q, i.value]])]),
				J("div", No, [t[10] ||= J("label", null, "Sender", -1), V(J("input", {
					"onUpdate:modelValue": t[2] ||= (e) => a.value = e,
					type: "text",
					class: "field-input",
					placeholder: "z.B. ARD, ZDF"
				}, null, 512), [[Q, a.value]])]),
				J("div", Po, [t[11] ||= J("label", null, "Kombinierte Suche", -1), V(J("input", {
					"onUpdate:modelValue": t[3] ||= (e) => o.value = e,
					type: "text",
					class: "field-input",
					placeholder: "Sucht in Titel und Thema"
				}, null, 512), [[Q, o.value]])]),
				J("div", Fo, [t[12] ||= J("label", null, "Min. Dauer (Minuten)", -1), V(J("input", {
					"onUpdate:modelValue": t[4] ||= (e) => s.value = e,
					type: "number",
					class: "field-input",
					placeholder: "0"
				}, null, 512), [[Q, s.value]])]),
				J("div", Io, [
					t[13] ||= J("label", null, "Max. Dauer (Minuten)", -1),
					t[14] ||= Y(),
					V(J("input", {
						"onUpdate:modelValue": t[5] ||= (e) => c.value = e,
						type: "number",
						class: "field-input",
						placeholder: "unbegrenzt"
					}, null, 512), [[Q, c.value]])
				]),
				J("div", Lo, [t[15] ||= J("label", null, "Von Datum", -1), V(J("input", {
					"onUpdate:modelValue": t[6] ||= (e) => l.value = e,
					type: "date",
					class: "field-input"
				}, null, 512), [[Q, l.value]])]),
				J("div", Ro, [t[16] ||= J("label", null, "Bis Datum", -1), V(J("input", {
					"onUpdate:modelValue": t[7] ||= (e) => u.value = e,
					type: "date",
					class: "field-input"
				}, null, 512), [[Q, u.value]])])
			]), J("div", zo, [J("button", {
				type: "submit",
				class: "btn btn-primary",
				disabled: f.value
			}, A(f.value ? "Suche läuft..." : "Suche starten"), 9, Bo), J("button", {
				type: "button",
				onClick: _,
				class: "btn btn-secondary btn-icon-only",
				title: "Abo aus Suche erstellen"
			}, " ➕ ")])], 32),
			d.value.length > 0 ? (K(), q("div", Vo, [J("h3", null, "Ergebnisse (" + A(d.value.length < 50 ? d.value.length : "50+") + ")", 1), (K(!0), q(W, null, br(d.value, (e) => (K(), q("div", {
				key: e.Id,
				class: "result-item"
			}, [J("div", Ho, [
				J("div", Uo, A(e.Title), 1),
				J("div", Wo, [Y(A(e.Channel) + " | " + A(e.Topic) + " | " + A(e.Duration) + " | ", 1), e.SubtitleUrls && e.SubtitleUrls.length > 0 ? (K(), q("span", Go)) : X("", !0)]),
				J("div", Ko, A(e.Description), 1)
			]), J("div", qo, [J("button", {
				onClick: (t) => g(e),
				class: "btn-icon",
				title: "Im Browser abspielen"
			}, "▶", 8, Jo), J("button", {
				onClick: (t) => v(e),
				class: "btn-icon",
				title: "Abo für diese Sendung erstellen"
			}, "➕", 8, Yo)])]))), 128))])) : !f.value && (r.value || i.value || a.value || o.value) ? (K(), q("div", Xo, " Keine Ergebnisse gefunden. ")) : X("", !0)
		]));
	}
}, [["__scopeId", "data-v-0f1b9dfa"]]), Qo = { class: "card" }, $o = { class: "field" }, es = ["disabled"], ts = "a31b415a-5264-419d-b152-8c8192a54994", ns = {
	__name: "SettingsTab",
	setup(e) {
		let t = window.ApiClient ?? null, n = /* @__PURE__ */ R("VueJS"), r = /* @__PURE__ */ R(!1);
		ur(async () => {
			t && (n.value = (await t.getPluginConfiguration(ts)).ActiveWebUi ?? "VueJS");
		});
		async function i() {
			r.value = !0;
			try {
				let e = await t.getPluginConfiguration(ts);
				e.ActiveWebUi = n.value, await t.updatePluginConfiguration(ts, e);
			} finally {
				r.value = !1;
			}
		}
		return (e, t) => (K(), q("div", Qo, [t[3] ||= J("h2", null, "Einstellungen", -1), J("form", { onSubmit: bo(i, ["prevent"]) }, [J("div", $o, [t[2] ||= J("label", {
			class: "field-label",
			for: "optActiveWebUi"
		}, "Aktive WebUI", -1), V(J("select", {
			id: "optActiveWebUi",
			"onUpdate:modelValue": t[0] ||= (e) => n.value = e,
			class: "field-select"
		}, [...t[1] ||= [
			J("option", { value: "VueJS" }, "VueJS", -1),
			J("option", { value: "ShowBoth" }, "Beide anzeigen", -1),
			J("option", { value: "Html" }, "HTML Legacy", -1)
		]], 512), [[mo, n.value]])]), J("button", {
			type: "submit",
			class: "btn btn-save",
			disabled: r.value
		}, "Speichern", 8, es)], 32)]));
	}
}, rs = { class: "card" }, is = { class: "header-row" }, as = ["disabled"], os = {
	key: 0,
	class: "state-msg"
}, ss = {
	key: 1,
	class: "error-container"
}, cs = { class: "error-msg" }, ls = {
	key: 2,
	class: "subscriptions-list"
}, us = { class: "sub-info" }, ds = { class: "sub-name" }, fs = {
	key: 0,
	class: "badge"
}, ps = { class: "sub-meta" }, ms = { class: "sub-actions" }, hs = ["onClick"], gs = ["onClick"], _s = ["onClick"], vs = {
	key: 3,
	class: "no-data"
}, ys = /* @__PURE__ */ Oo({
	__name: "SubscriptionsTab",
	props: { onEdit: {
		type: Function,
		required: !0
	} },
	setup(e, { expose: t }) {
		let n = window.ApiClient ?? null, r = window.Dashboard ?? null, i = /* @__PURE__ */ R([]), a = /* @__PURE__ */ R(!1), o = /* @__PURE__ */ R(null);
		async function s() {
			if (n) {
				a.value = !0, o.value = null;
				try {
					let e = n.getUrl("MediathekViewDL/Subscriptions");
					i.value = await n.getJSON(e);
				} catch (e) {
					o.value = "Fehler beim Laden der Abonnements.", console.error("Failed to fetch subscriptions", e);
				} finally {
					a.value = !1;
				}
			}
		}
		async function c(e) {
			!n || !r || r.confirm("Soll dieses Abonnement wirklich gelöscht werden?", "Löschen bestätigen", async (t) => {
				if (t) try {
					let t = n.getUrl("MediathekViewDL/Subscriptions/" + e);
					await n.ajax({
						type: "DELETE",
						url: t
					}), await s(), r.alert("Abonnement gelöscht.");
				} catch (e) {
					console.error("Delete failed", e), r.alert("Fehler beim Löschen des Abonnements.");
				}
			});
		}
		async function l(e) {
			if (!(!n || !r)) try {
				let t = n.getUrl("MediathekViewDL/Subscriptions/" + e + "/Process"), i = await n.ajax({
					type: "POST",
					url: t
				});
				r.alert(i + " neue Elemente gefunden.");
			} catch (e) {
				console.error("Processing failed", e), r.alert("Fehler beim Verarbeiten.");
			}
		}
		return ur(() => {
			s();
		}), t({ refresh: s }), (t, n) => (K(), q("div", rs, [J("div", is, [n[1] ||= J("h2", null, "Abo Verwaltung", -1), J("button", {
			class: "btn btn-primary",
			onClick: n[0] ||= (t) => e.onEdit(),
			disabled: a.value
		}, "Neues Abo", 8, as)]), a.value ? (K(), q("div", os, [...n[2] ||= [J("div", { class: "spinner" }, null, -1), Y(" Lade Abonnements... ", -1)]])) : o.value ? (K(), q("div", ss, [J("div", cs, A(o.value), 1), J("button", {
			onClick: s,
			class: "btn btn-secondary"
		}, "Erneut versuchen")])) : i.value.length > 0 ? (K(), q("div", ls, [(K(!0), q(W, null, br(i.value, (t) => (K(), q("div", {
			key: t.Id,
			class: k(["subscription-item", { disabled: !t.IsEnabled }])
		}, [J("div", us, [J("div", ds, [Y(A(t.Name) + " ", 1), t.IsEnabled ? X("", !0) : (K(), q("span", fs, "Deaktiviert"))]), J("div", ps, " Letzter Download: " + A(t.LastDownloadedTimestamp ? new Date(t.LastDownloadedTimestamp).toLocaleString() : "Nie"), 1)]), J("div", ms, [
			J("button", {
				onClick: (e) => l(t.Id),
				class: "btn-icon",
				title: "Jetzt verarbeiten"
			}, "🔄", 8, hs),
			J("button", {
				onClick: (n) => e.onEdit(t),
				class: "btn-icon",
				title: "Bearbeiten"
			}, "✏️", 8, gs),
			J("button", {
				onClick: (e) => c(t.Id),
				class: "btn-icon btn-delete",
				title: "Löschen"
			}, "🗑️", 8, _s)
		])], 2))), 128))])) : (K(), q("div", vs, " Keine Abonnements konfiguriert. "))]));
	}
}, [["__scopeId", "data-v-733f9dde"]]), bs = {}, xs = { class: "card" };
function Ss(e, t) {
	return K(), q("div", xs, [...t[0] ||= [J("h2", null, "Downloads", -1), J("p", null, "Download-Verlauf kommt hierhin.", -1)]]);
}
var Cs = /* @__PURE__ */ Oo(bs, [["render", Ss]]), ws = {
	key: 0,
	class: "editor-overlay"
}, Ts = { class: "editor-modal card" }, Es = { class: "editor-header" }, Ds = { class: "editor-tabs" }, Os = { class: "editor-content" }, ks = {
	key: 0,
	class: "tab-pane"
}, As = { class: "field" }, js = { class: "checkbox-field" }, Ms = { class: "checkbox-field" }, Ns = { class: "checkbox-field" }, Ps = {
	key: 1,
	class: "tab-pane"
}, Fs = { class: "query-fields" }, Is = ["onClick"], Ls = { class: "query-input-row" }, Rs = ["onUpdate:modelValue", "placeholder"], zs = ["onClick"], Bs = ["onClick"], Vs = { class: "grid-2" }, Hs = { class: "field" }, Us = { class: "field" }, Ws = { class: "grid-2" }, Gs = { class: "field" }, Ks = ["value"], qs = { class: "field" }, Js = ["value"], Ys = {
	key: 2,
	class: "tab-pane"
}, Xs = { class: "field" }, Zs = { class: "input-with-btn" }, Qs = { class: "checkbox-field" }, $s = {
	key: 0,
	class: "sub-options"
}, ec = { class: "checkbox-field" }, tc = { class: "checkbox-field" }, nc = { class: "checkbox-field" }, rc = { class: "checkbox-field" }, ic = {
	key: 1,
	class: "sub-options"
}, ac = { class: "checkbox-field" }, oc = {
	key: 3,
	class: "tab-pane"
}, sc = { class: "checkbox-field" }, cc = {
	key: 0,
	class: "sub-options"
}, lc = { class: "checkbox-field" }, uc = {
	key: 1,
	class: "sub-options"
}, dc = { class: "checkbox-field" }, fc = {
	key: 0,
	class: "sub-options"
}, pc = { class: "checkbox-field" }, mc = { class: "checkbox-field" }, hc = { class: "checkbox-field" }, gc = { class: "checkbox-field" }, _c = {
	key: 4,
	class: "tab-pane"
}, vc = { class: "field" }, yc = { class: "checkbox-field" }, bc = { class: "checkbox-field" }, xc = {
	key: 0,
	class: "sub-options"
}, Sc = { class: "checkbox-field" }, Cc = { class: "checkbox-field" }, wc = {
	key: 5,
	class: "tab-pane"
}, Tc = { class: "checkbox-field" }, Ec = { class: "checkbox-field" }, Dc = { class: "editor-footer" }, Oc = /* @__PURE__ */ Oo({
	__name: "SubscriptionEditor",
	props: { subscription: {
		type: Object,
		default: null
	} },
	emits: ["save", "cancel"],
	setup(e, { emit: t }) {
		let n = e, r = t, i = /* @__PURE__ */ R(null), a = /* @__PURE__ */ R("basic");
		window.ApiClient;
		let o = window.Dashboard ?? null;
		An(() => n.subscription, (e) => {
			e ? i.value = JSON.parse(JSON.stringify(e)) : i.value = null;
		}, { immediate: !0 });
		function s() {
			i.value.Search.Criteria.push({
				Fields: ["Title", "Topic"],
				Query: "",
				IsExclude: !1
			});
		}
		function c(e) {
			i.value.Search.Criteria.splice(e, 1);
		}
		function l(e, t) {
			let n = e.Fields.indexOf(t);
			n > -1 ? e.Fields.length > 1 && e.Fields.splice(n, 1) : e.Fields.push(t);
		}
		async function u() {
			r("save", i.value);
		}
		function d() {
			r("cancel");
		}
		function f() {
			if (!o) return;
			let e = new o.DirectoryBrowser();
			e.show({
				header: "Abo Pfad wählen",
				includeDirectories: !0,
				includeFiles: !1,
				callback: (t) => {
					t && (i.value.Download || (i.value.Download = { DownloadPath: "" }), i.value.Download.DownloadPath = t), e.close();
				}
			});
		}
		function p(e) {
			return e ? e.split("T")[0] : "";
		}
		function m(e, t, n) {
			if (!n) {
				e[t] = null;
				return;
			}
			let r = new Date(n);
			t === "MaxBroadcastDate" && r.setHours(23, 59, 59, 999), e[t] = r.toISOString();
		}
		return (e, t) => i.value ? (K(), q("div", ws, [J("div", Ts, [
			J("header", Es, [J("h2", null, A(i.value.Id ? "Abonnement bearbeiten" : "Neues Abonnement"), 1), J("div", { class: "header-actions" }, [J("button", {
				onClick: d,
				class: "btn-icon"
			}, "✕")])]),
			J("div", Ds, [
				J("button", {
					class: k(["tab-btn", { active: a.value === "basic" }]),
					onClick: t[0] ||= (e) => a.value = "basic"
				}, "Allgemein", 2),
				J("button", {
					class: k(["tab-btn", { active: a.value === "search" }]),
					onClick: t[1] ||= (e) => a.value = "search"
				}, "Suche", 2),
				J("button", {
					class: k(["tab-btn", { active: a.value === "download" }]),
					onClick: t[2] ||= (e) => a.value = "download"
				}, "Download", 2),
				J("button", {
					class: k(["tab-btn", { active: a.value === "series" }]),
					onClick: t[3] ||= (e) => a.value = "series"
				}, "Serien", 2),
				J("button", {
					class: k(["tab-btn", { active: a.value === "metadata" }]),
					onClick: t[4] ||= (e) => a.value = "metadata"
				}, "Metadaten", 2),
				J("button", {
					class: k(["tab-btn", { active: a.value === "accessibility" }]),
					onClick: t[5] ||= (e) => a.value = "accessibility"
				}, "Barrierefreiheit", 2)
			]),
			J("div", Os, [
				a.value === "basic" ? (K(), q("div", ks, [
					J("div", As, [t[36] ||= J("label", null, "Name (Serienname)", -1), V(J("input", {
						"onUpdate:modelValue": t[6] ||= (e) => i.value.Name = e,
						type: "text",
						class: "field-input",
						placeholder: "z.B. Tatort",
						required: ""
					}, null, 512), [[Q, i.value.Name]])]),
					J("div", js, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[7] ||= (e) => i.value.IsEnabled = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.IsEnabled]]), t[37] ||= Y(" Aktiviert ", -1)])]),
					J("div", Ms, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[8] ||= (e) => i.value.IgnoreLocalFiles = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.IgnoreLocalFiles]]), t[38] ||= Y(" Lokale Dateien ignorieren ", -1)]), t[39] ||= J("p", { class: "field-desc" }, "Erzwingt den Download, auch wenn die Datei bereits lokal existiert.", -1)]),
					J("div", Ns, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[9] ||= (e) => i.value.IgnoreHistory = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.IgnoreHistory]]), t[40] ||= Y(" Download-Verlauf ignorieren ", -1)]), t[41] ||= J("p", { class: "field-desc" }, "Erzwingt den Download, auch wenn die Sendung bereits früher geladen wurde.", -1)])
				])) : X("", !0),
				a.value === "search" ? (K(), q("div", Ps, [
					t[46] ||= J("h3", null, "Suchanfragen", -1),
					(K(!0), q(W, null, br(i.value.Search.Criteria, (e, t) => (K(), q("div", {
						key: t,
						class: "query-row"
					}, [J("div", Fs, [(K(), q(W, null, br([
						"Title",
						"Topic",
						"Description",
						"Channel"
					], (t) => J("button", {
						key: t,
						onClick: (n) => l(e, t),
						class: k(["field-tag", { active: e.Fields.includes(t) }])
					}, A(t === "Title" ? "Titel" : t === "Topic" ? "Thema" : t === "Description" ? "Beschreibung" : "Sender"), 11, Is)), 64))]), J("div", Ls, [
						V(J("input", {
							"onUpdate:modelValue": (t) => e.Query = t,
							type: "text",
							class: "field-input",
							placeholder: e.IsExclude ? "Ausschließen..." : "Suchen..."
						}, null, 8, Rs), [[Q, e.Query]]),
						J("button", {
							onClick: (t) => e.IsExclude = !e.IsExclude,
							class: k(["btn-small", { "btn-danger": e.IsExclude }])
						}, A(e.IsExclude ? "NICHT" : "SUCHE"), 11, zs),
						J("button", {
							onClick: (e) => c(t),
							class: "btn-icon"
						}, "🗑️", 8, Bs)
					])]))), 128)),
					J("button", {
						onClick: s,
						class: "btn btn-secondary"
					}, "Anfrage hinzufügen"),
					t[47] ||= J("hr", null, null, -1),
					J("div", Vs, [J("div", Hs, [t[42] ||= J("label", null, "Min. Dauer (Minuten)", -1), V(J("input", {
						"onUpdate:modelValue": t[10] ||= (e) => i.value.Search.MinDurationMinutes = e,
						type: "number",
						class: "field-input"
					}, null, 512), [[Q, i.value.Search.MinDurationMinutes]])]), J("div", Us, [t[43] ||= J("label", null, "Max. Dauer (Minuten)", -1), V(J("input", {
						"onUpdate:modelValue": t[11] ||= (e) => i.value.Search.MaxDurationMinutes = e,
						type: "number",
						class: "field-input"
					}, null, 512), [[Q, i.value.Search.MaxDurationMinutes]])])]),
					J("div", Ws, [J("div", Gs, [t[44] ||= J("label", null, "Min. Sendedatum", -1), J("input", {
						value: p(i.value.Search.MinBroadcastDate),
						onInput: t[12] ||= (e) => m(i.value.Search, "MinBroadcastDate", e.target.value),
						type: "date",
						class: "field-input"
					}, null, 40, Ks)]), J("div", qs, [t[45] ||= J("label", null, "Max. Sendedatum", -1), J("input", {
						value: p(i.value.Search.MaxBroadcastDate),
						onInput: t[13] ||= (e) => m(i.value.Search, "MaxBroadcastDate", e.target.value),
						type: "date",
						class: "field-input"
					}, null, 40, Js)])])
				])) : X("", !0),
				a.value === "download" ? (K(), q("div", Ys, [
					J("div", Xs, [
						t[48] ||= J("label", null, "Download Pfad (Optional)", -1),
						J("div", Zs, [V(J("input", {
							"onUpdate:modelValue": t[14] ||= (e) => i.value.Download.DownloadPath = e,
							type: "text",
							class: "field-input",
							placeholder: "Wenn leer werden die Standardpfade verwendet"
						}, null, 512), [[Q, i.value.Download.DownloadPath]]), J("button", {
							onClick: f,
							class: "btn btn-secondary"
						}, "Wählen")]),
						t[49] ||= J("p", { class: "field-desc" }, "Leer lassen, um den Standardpfad zu nutzen. Bei Serien wird automatisch ein Unterordner mit dem Abo-Namen erstellt. Bei Filmen wird ein Unterordner mit dem Abo-Namen nur erstellt, wenn die Option \"Ordner für das Thema erstellen\" in den Einstellungen aktiviert ist.", -1)
					]),
					J("div", Qs, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[15] ||= (e) => i.value.Download.UseStreamingUrlFiles = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Download.UseStreamingUrlFiles]]), t[50] ||= Y(" Streaming-URL-Dateien (.strm) verwenden ", -1)]), t[51] ||= J("p", { class: "field-desc" }, "Verwendet Streaming-URL-Dateien (.strm) anstelle des Herunterladens der tatsächlichen Videodateien. Es werden keine Videodateien gespeichert, die Videos werden von ARD/ZDF direkt gestreamt. Untertitel sind hiervon nicht betroffen.", -1)]),
					i.value.Download.UseStreamingUrlFiles ? X("", !0) : (K(), q("div", $s, [J("div", ec, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[16] ||= (e) => i.value.Download.DownloadFullVideoForSecondaryAudio = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Download.DownloadFullVideoForSecondaryAudio]]), t[52] ||= Y(" Vollständiges Video für sekundäre Audiosprachen herunterladen ", -1)]), t[53] ||= J("p", { class: "field-desc" }, "Wenn aktiviert, wird das vollständige Video heruntergeladen, auch wenn es eine andere Audiosprache als Deutsch enthält. Andernfalls wird nur die Audiospur dieser Sprache extrahiert.", -1)])])),
					J("div", tc, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[17] ||= (e) => i.value.Download.AlwaysCreateSubfolder = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Download.AlwaysCreateSubfolder]]), t[54] ||= Y(" Unterordner für dieses Abo erstellen ", -1)]), t[55] ||= J("p", { class: "field-desc" }, "Erstellt immer einen Unterordner mit dem Namen des Abonnements, auch wenn es sich um Filme handelt und die globale Einstellung \"Beim Film Downloads Ordner für das Thema erstellen\" deaktiviert ist.", -1)]),
					J("div", nc, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[18] ||= (e) => i.value.Download.EnhancedDuplicateDetection = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Download.EnhancedDuplicateDetection]]), t[56] ||= Y(" Erweiterte Duplikaterkennung ", -1)]), t[57] ||= J("p", { class: "field-desc" }, "Scannt das Zielverzeichnis nach vorhandenen Dateien mit passenden SxxExx-Mustern (oder absoluter Nummerierung), um doppelte Downloads zu vermeiden (auch bei abweichenden Dateinamen).", -1)]),
					J("div", rc, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[19] ||= (e) => i.value.Download.AllowFallbackToLowerQuality = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Download.AllowFallbackToLowerQuality]]), t[58] ||= Y(" Fallback auf niedrigere Qualität erlauben ", -1)]), t[59] ||= J("p", { class: "field-desc" }, "Wenn aktiviert, wird beim Herunterladen einer Episode geprüft, ob eine niedrigere Qualität verfügbar ist falls die HD-URL nicht gesetzt ist.", -1)]),
					i.value.Download.AllowFallbackToLowerQuality ? (K(), q("div", ic, [J("div", ac, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[20] ||= (e) => i.value.Download.QualityCheckWithUrl = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Download.QualityCheckWithUrl]]), t[60] ||= Y(" Prüft ob die URLs gültig ist. ", -1)]), t[61] ||= J("p", { class: "field-desc" }, "Wenn aktiviert wird auch geprüft, ob die URLs von MediathekView noch verfügbar sind und ggf. die nächst niedrigere versucht. HD → Default → SD", -1)])])) : X("", !0)
				])) : X("", !0),
				a.value === "series" ? (K(), q("div", oc, [J("div", sc, [J("label", null, [V(J("input", {
					"onUpdate:modelValue": t[21] ||= (e) => i.value.Series.EnforceSeriesParsing = e,
					type: "checkbox"
				}, null, 512), [[$, i.value.Series.EnforceSeriesParsing]]), t[62] ||= Y(" Nur Serien herunterladen ", -1)]), t[63] ||= J("p", { class: "field-desc" }, "Nur Videos herunterladen, die als Serie erkannt werden", -1)]), i.value.Series.EnforceSeriesParsing ? (K(), q("div", cc, [J("div", lc, [J("label", null, [V(J("input", {
					"onUpdate:modelValue": t[22] ||= (e) => i.value.Series.AllowAbsoluteEpisodeNumbering = e,
					type: "checkbox"
				}, null, 512), [[$, i.value.Series.AllowAbsoluteEpisodeNumbering]]), t[64] ||= Y(" Absolute Episodennummerierung erlauben ", -1)]), t[65] ||= J("p", { class: "field-desc" }, "Episoden auch herunterladen, wenn nur Absolute Episodennummerierung vorliegt (z.B. \"Episode 5\" statt \"Staffel 1, Episode 5\").", -1)])])) : (K(), q("div", uc, [J("div", dc, [J("label", null, [V(J("input", {
					"onUpdate:modelValue": t[23] ||= (e) => i.value.Series.TreatNonEpisodesAsExtras = e,
					type: "checkbox"
				}, null, 512), [[$, i.value.Series.TreatNonEpisodesAsExtras]]), t[66] ||= Y(" Nicht Episoden als Extras behandeln ", -1)]), t[67] ||= J("p", { class: "field-desc" }, "Nicht als Episoden erkannte Videos als Extras behandeln.", -1)]), i.value.Series.TreatNonEpisodesAsExtras ? (K(), q("div", fc, [
					J("div", pc, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[24] ||= (e) => i.value.Series.SaveTrailers = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Series.SaveTrailers]]), t[68] ||= Y(" Trailer speichern", -1)]), t[69] ||= J("p", { class: "field-desc" }, "Trailer werden gespeichert.", -1)]),
					J("div", mc, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[25] ||= (e) => i.value.Series.SaveInterviews = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Series.SaveInterviews]]), t[70] ||= Y(" Interviews speichern", -1)]), t[71] ||= J("p", { class: "field-desc" }, "Interviews werden gespeichert.", -1)]),
					J("div", hc, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[26] ||= (e) => i.value.Series.SaveGenericExtras = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Series.SaveGenericExtras]]), t[72] ||= Y(" Generische Extras speichern", -1)]), t[73] ||= J("p", { class: "field-desc" }, "Alle anderen Extras (nicht Trailer/Interviews) werden gespeichert.", -1)]),
					J("div", gc, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[27] ||= (e) => i.value.Series.SaveExtrasAsStrm = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Series.SaveExtrasAsStrm]]), t[74] ||= Y(" Extras als Stream (.strm) speichern", -1)]), t[75] ||= J("p", { class: "field-desc" }, "Extras werden als .strm Dateien gespeichert (spart Speicherplatz).", -1)])
				])) : X("", !0)]))])) : X("", !0),
				a.value === "metadata" ? (K(), q("div", _c, [
					J("div", vc, [
						t[76] ||= J("label", null, "Originalsprache (ISO Code, z.B. 'eng')", -1),
						V(J("input", {
							"onUpdate:modelValue": t[28] ||= (e) => i.value.Metadata.OriginalLanguage = e,
							type: "text",
							class: "field-input",
							placeholder: "z.B. eng oder fra"
						}, null, 512), [[Q, i.value.Metadata.OriginalLanguage]]),
						t[77] ||= J("p", { class: "field-desc" }, "Falls gesetzt, wird dieser Sprachcode verwendet, wenn der Inhalt als Originalversion (OV/OmU) erkannt wird (statt 'und').", -1)
					]),
					J("div", yc, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[29] ||= (e) => i.value.Metadata.CreateNfo = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Metadata.CreateNfo]]), t[78] ||= Y(" NFO Dateien erstellen ", -1)]), t[79] ||= J("p", { class: "field-desc" }, "Erstellt eine .nfo Datei mit Metadaten (Beschreibung, Episodennummer) neben der Videodatei.", -1)]),
					J("div", bc, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[30] ||= (e) => i.value.Metadata.AppendDateToTitle = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Metadata.AppendDateToTitle]]), t[80] ||= Y(" Datum an Titel anhängen ", -1)]), t[81] ||= J("p", { class: "field-desc" }, "Hängt das Sendedatum an den Titel an (z.B. \"Titel - 2026-01-01\") und erzwingt die Erkennung als Serie. Nützlich für Sendungen wie \"Tagesschau in 100 Sekunden\", die kein Release-Datum im Titel haben.", -1)]),
					i.value.Metadata.AppendDateToTitle ? (K(), q("div", xc, [J("div", Sc, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[31] ||= (e) => i.value.Metadata.AppendTimeToTitle = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Metadata.AppendTimeToTitle]]), t[82] ||= Y(" Uhrzeit an Titel anhängen ", -1)]), t[83] ||= J("p", { class: "field-desc" }, "Hängt die Uhrzeit an den Titel an (z.B. \"Titel - 2026-01-01 20-00\").", -1)])])) : X("", !0),
					J("div", Cc, [J("label", null, [V(J("input", {
						"onUpdate:modelValue": t[32] ||= (e) => i.value.Metadata.KeepOriginalTitle = e,
						type: "checkbox"
					}, null, 512), [[$, i.value.Metadata.KeepOriginalTitle]]), t[84] ||= Y(" Originaltitel beibehalten ", -1)]), t[85] ||= J("p", { class: "field-desc" }, "Behält den Originaltitel bei und entfernt keine Informationen wie (AD), Gebärdensprache oder Episodennummern aus dem Titel.", -1)])
				])) : X("", !0),
				a.value === "accessibility" ? (K(), q("div", wc, [J("div", Tc, [J("label", null, [V(J("input", {
					"onUpdate:modelValue": t[33] ||= (e) => i.value.Accessibility.AllowAudioDescription = e,
					type: "checkbox"
				}, null, 512), [[$, i.value.Accessibility.AllowAudioDescription]]), t[86] ||= Y(" Versionen mit Audiodeskription herunterladen ", -1)]), t[87] ||= J("p", { class: "field-desc" }, "Lädt auch Inhalte mit Audiodeskription herunter (sofern verfügbar).", -1)]), J("div", Ec, [J("label", null, [V(J("input", {
					"onUpdate:modelValue": t[34] ||= (e) => i.value.Accessibility.AllowSignLanguage = e,
					type: "checkbox"
				}, null, 512), [[$, i.value.Accessibility.AllowSignLanguage]]), t[88] ||= Y(" Versionen mit Gebärdensprache herunterladen ", -1)]), t[89] ||= J("p", { class: "field-desc" }, "Lädt auch Inhalte mit Gebärdensprache herunter. (sofern verfügbar).", -1)])])) : X("", !0)
			]),
			J("footer", Dc, [
				J("button", {
					onClick: d,
					class: "btn btn-secondary"
				}, "Abbrechen"),
				J("button", {
					onClick: t[35] ||= (t) => e.$emit("test", i.value),
					class: "btn btn-secondary"
				}, "Abo prüfen (Dry Run)"),
				J("button", {
					onClick: u,
					class: "btn btn-primary"
				}, "Abo Speichern")
			])
		])])) : X("", !0);
	}
}, [["__scopeId", "data-v-a6c8dd1b"]]), kc = { class: "plugin-config" }, Ac = { class: "tab-row" }, jc = { class: "tab-content" }, Mc = {
	key: 0,
	class: "modal-overlay"
}, Nc = { class: "modal-card test-modal card" }, Pc = { class: "modal-header" }, Fc = { class: "modal-content" }, Ic = {
	key: 0,
	class: "state-msg"
}, Lc = {
	key: 1,
	class: "no-data"
}, Rc = {
	key: 2,
	class: "test-results-list"
}, zc = { class: "test-item-title" }, Bc = { class: "test-item-meta" }, Vc = { class: "test-item-meta" }, Hc = { class: "modal-footer" }, Uc = "a31b415a-5264-419d-b152-8c8192a54994", Wc = /* @__PURE__ */ Oo({
	__name: "PluginConfig",
	setup(e) {
		let t = window.ApiClient ?? null, n = window.Dashboard ?? null, r = /* @__PURE__ */ R("search"), i = /* @__PURE__ */ R(null), a = /* @__PURE__ */ R(null), o = /* @__PURE__ */ R(!1), s = /* @__PURE__ */ R([]), c = /* @__PURE__ */ R(!1);
		async function l() {
			t && (i.value = await t.getPluginConfiguration(Uc));
		}
		function u(e = null) {
			if (e) a.value = e;
			else {
				let e = i.value?.SubscriptionDefaults || {};
				a.value = Do.createDefault(e);
			}
		}
		async function d(e) {
			if (t) try {
				let r = !e.Id, i = r ? t.getUrl("MediathekViewDL/Subscriptions") : t.getUrl("MediathekViewDL/Subscriptions/" + e.Id);
				await t.ajax({
					type: r ? "POST" : "PUT",
					url: i,
					data: JSON.stringify(e),
					contentType: "application/json"
				}), a.value = null, n && n.alert("Abonnement gespeichert.");
			} catch (e) {
				console.error("Save failed", e), n && n.alert("Fehler beim Speichern des Abonnements.");
			}
		}
		async function f(e) {
			if (t) {
				s.value = [], c.value = !0, o.value = !0;
				try {
					let n = t.getUrl("MediathekViewDL/Subscriptions/Test"), r = await t.ajax({
						type: "POST",
						url: n,
						data: JSON.stringify(e),
						contentType: "application/json",
						dataType: "json"
					}), i = [];
					Array.isArray(r) ? i = r : r && Array.isArray(r.Items) ? i = r.Items : r && Array.isArray(r.data) && (i = r.data), s.value = i;
				} catch (e) {
					console.error("Test failed", e), n && n.alert("Fehler beim Testen des Abonnements."), o.value = !1;
				} finally {
					c.value = !1;
				}
			}
		}
		return ur(() => {
			l();
		}), (e, t) => (K(), q("div", kc, [
			t[9] ||= J("header", { class: "config-header" }, [J("h1", { class: "config-title" }, "MediathekViewDL"), J("p", { class: "config-subtitle" }, "Plugin Konfiguration")], -1),
			J("div", Ac, [
				J("button", {
					class: k(["tab-btn", { active: r.value === "search" }]),
					onClick: t[0] ||= (e) => r.value = "search"
				}, "Suche", 2),
				J("button", {
					class: k(["tab-btn", { active: r.value === "settings" }]),
					onClick: t[1] ||= (e) => r.value = "settings"
				}, "Einstellungen", 2),
				J("button", {
					class: k(["tab-btn", { active: r.value === "subscriptions" }]),
					onClick: t[2] ||= (e) => r.value = "subscriptions"
				}, "Abos", 2),
				J("button", {
					class: k(["tab-btn", { active: r.value === "downloads" }]),
					onClick: t[3] ||= (e) => r.value = "downloads"
				}, "Downloads", 2)
			]),
			J("div", jc, [
				r.value === "search" ? (K(), Bi(Zo, {
					key: 0,
					onCreateSub: u
				})) : X("", !0),
				r.value === "settings" ? (K(), Bi(ns, { key: 1 })) : X("", !0),
				r.value === "subscriptions" ? (K(), Bi(ys, {
					key: 2,
					"on-edit": u
				})) : X("", !0),
				r.value === "downloads" ? (K(), Bi(Cs, { key: 3 })) : X("", !0)
			]),
			(K(), Bi(Gn, { to: "body" }, [Gi(Oc, {
				subscription: a.value,
				onSave: d,
				onTest: f,
				onCancel: t[4] ||= (e) => a.value = null
			}, null, 8, ["subscription"])])),
			(K(), Bi(Gn, { to: "body" }, [o.value ? (K(), q("div", Mc, [J("div", Nc, [
				J("header", Pc, [t[7] ||= J("h2", null, "Abo-Test Ergebnisse", -1), J("button", {
					onClick: t[5] ||= (e) => o.value = !1,
					class: "btn-icon"
				}, "✕")]),
				J("div", Fc, [c.value ? (K(), q("div", Ic, [...t[8] ||= [J("div", { class: "spinner" }, null, -1), Y(" Suche nach Treffern... ", -1)]])) : s.value.length === 0 ? (K(), q("div", Lc, " Keine Sendungen gefunden. ")) : (K(), q("div", Rc, [J("p", null, "Folgende " + A(s.value.length) + " Sendungen würden heruntergeladen werden:", 1), (K(!0), q(W, null, br(s.value, (e, t) => (K(), q("div", {
					key: t,
					class: "test-item"
				}, [
					J("div", zc, A(e.Title), 1),
					J("div", Bc, A(e.Channel) + " | " + A(e.Topic) + " | " + A(e.Duration), 1),
					J("div", Vc, A(e.Description), 1)
				]))), 128))]))]),
				J("footer", Hc, [J("button", {
					onClick: t[6] ||= (e) => o.value = !1,
					class: "btn btn-primary"
				}, "Schließen")])
			])])) : X("", !0)]))
		]));
	}
}, [["__scopeId", "data-v-b10468a8"]]), Gc = {
	__name: "App",
	setup(e) {
		return (e, t) => (K(), Bi(Wc));
	}
}, Kc = null;
function qc(e) {
	Kc || (Kc = wo(Gc), Kc.mount(e));
}
function Jc() {
	Kc &&= (Kc.unmount(), null);
}
var Yc = document.getElementById("configPageVueJSPage"), Xc = !window.Dashboard;
if (Yc) if (Xc) {
	let e = Yc.querySelector("#app");
	e && qc(e);
} else Yc.addEventListener("pageshow", (e) => {
	let t = Yc.querySelector("#app");
	t && (Jc(), qc(t));
}), Yc.addEventListener("pagehide", () => {
	Jc();
});
//#endregion
