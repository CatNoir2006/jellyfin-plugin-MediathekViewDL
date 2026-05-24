(function(){
          const style = document.createElement('style');
          style.textContent = ".plugin-config[data-v-a62a1791]{color:#e4e4e7;box-sizing:border-box;width:100%;margin:0 auto;padding:1rem}.config-header[data-v-a62a1791]{margin-bottom:2rem}.tab-row[data-v-a62a1791]{border-bottom:1px solid #333;gap:10px;margin-bottom:20px;padding-bottom:10px;display:flex}.tab-btn[data-v-a62a1791]{color:#a1a1aa;cursor:pointer;background:0 0;border:none;padding:10px;font-weight:600}.tab-btn.active[data-v-a62a1791]{color:#7c3aed;border-bottom:2px solid #7c3aed}\n/*$vite$:1*/";
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
//#endregion
//#region node_modules/@vue/reactivity/dist/reactivity.esm-bundler.js
var A, xe = class {
	constructor(e = !1) {
		this.detached = e, this._active = !0, this._on = 0, this.effects = [], this.cleanups = [], this._isPaused = !1, this._warnOnRun = !0, this.__v_skip = !0, !e && A && (A.active ? (this.parent = A, this.index = (A.scopes ||= []).push(this) - 1) : (this._active = !1, this._warnOnRun = !1));
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
			let t = A;
			try {
				return A = this, e();
			} finally {
				A = t;
			}
		}
	}
	on() {
		++this._on === 1 && (this.prevScope = A, A = this);
	}
	off() {
		if (this._on > 0 && --this._on === 0) {
			if (A === this) A = this.prevScope;
			else {
				let e = A;
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
function Se() {
	return A;
}
var j, Ce = /* @__PURE__ */ new WeakSet(), we = class {
	constructor(e) {
		this.fn = e, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, A && (A.active ? A.effects.push(this) : this.flags &= -2);
	}
	pause() {
		this.flags |= 64;
	}
	resume() {
		this.flags & 64 && (this.flags &= -65, Ce.has(this) && (Ce.delete(this), this.trigger()));
	}
	notify() {
		this.flags & 2 && !(this.flags & 32) || this.flags & 8 || Oe(this);
	}
	run() {
		if (!(this.flags & 1)) return this.fn();
		this.flags |= 2, Be(this), je(this);
		let e = j, t = M;
		j = this, M = !0;
		try {
			return this.fn();
		} finally {
			Me(this), j = e, M = t, this.flags &= -3;
		}
	}
	stop() {
		if (this.flags & 1) {
			for (let e = this.deps; e; e = e.nextDep) Fe(e);
			this.deps = this.depsTail = void 0, Be(this), this.onStop && this.onStop(), this.flags &= -2;
		}
	}
	trigger() {
		this.flags & 64 ? Ce.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
	}
	runIfDirty() {
		Ne(this) && this.run();
	}
	get dirty() {
		return Ne(this);
	}
}, Te = 0, Ee, De;
function Oe(e, t = !1) {
	if (e.flags |= 8, t) {
		e.next = De, De = e;
		return;
	}
	e.next = Ee, Ee = e;
}
function ke() {
	Te++;
}
function Ae() {
	if (--Te > 0) return;
	if (De) {
		let e = De;
		for (De = void 0; e;) {
			let t = e.next;
			e.next = void 0, e.flags &= -9, e = t;
		}
	}
	let e;
	for (; Ee;) {
		let t = Ee;
		for (Ee = void 0; t;) {
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
function je(e) {
	for (let t = e.deps; t; t = t.nextDep) t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function Me(e) {
	let t, n = e.depsTail, r = n;
	for (; r;) {
		let e = r.prevDep;
		r.version === -1 ? (r === n && (n = e), Fe(r), Ie(r)) : t = r, r.dep.activeLink = r.prevActiveLink, r.prevActiveLink = void 0, r = e;
	}
	e.deps = t, e.depsTail = n;
}
function Ne(e) {
	for (let t = e.deps; t; t = t.nextDep) if (t.dep.version !== t.version || t.dep.computed && (Pe(t.dep.computed) || t.dep.version !== t.version)) return !0;
	return !!e._dirty;
}
function Pe(e) {
	if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === Ve) || (e.globalVersion = Ve, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Ne(e)))) return;
	e.flags |= 2;
	let t = e.dep, n = j, r = M;
	j = e, M = !0;
	try {
		je(e);
		let n = e.fn(e._value);
		(t.version === 0 || D(n, e._value)) && (e.flags |= 128, e._value = n, t.version++);
	} catch (e) {
		throw t.version++, e;
	} finally {
		j = n, M = r, Me(e), e.flags &= -3;
	}
}
function Fe(e, t = !1) {
	let { dep: n, prevSub: r, nextSub: i } = e;
	if (r && (r.nextSub = i, e.prevSub = void 0), i && (i.prevSub = r, e.nextSub = void 0), n.subs === e && (n.subs = r, !r && n.computed)) {
		n.computed.flags &= -5;
		for (let e = n.computed.deps; e; e = e.nextDep) Fe(e, !0);
	}
	!t && !--n.sc && n.map && n.map.delete(n.key);
}
function Ie(e) {
	let { prevDep: t, nextDep: n } = e;
	t && (t.nextDep = n, e.prevDep = void 0), n && (n.prevDep = t, e.nextDep = void 0);
}
var M = !0, Le = [];
function Re() {
	Le.push(M), M = !1;
}
function ze() {
	let e = Le.pop();
	M = e === void 0 ? !0 : e;
}
function Be(e) {
	let { cleanup: t } = e;
	if (e.cleanup = void 0, t) {
		let e = j;
		j = void 0;
		try {
			t();
		} finally {
			j = e;
		}
	}
}
var Ve = 0, He = class {
	constructor(e, t) {
		this.sub = e, this.dep = t, this.version = t.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
	}
}, Ue = class {
	constructor(e) {
		this.computed = e, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
	}
	track(e) {
		if (!j || !M || j === this.computed) return;
		let t = this.activeLink;
		if (t === void 0 || t.sub !== j) t = this.activeLink = new He(j, this), j.deps ? (t.prevDep = j.depsTail, j.depsTail.nextDep = t, j.depsTail = t) : j.deps = j.depsTail = t, We(t);
		else if (t.version === -1 && (t.version = this.version, t.nextDep)) {
			let e = t.nextDep;
			e.prevDep = t.prevDep, t.prevDep && (t.prevDep.nextDep = e), t.prevDep = j.depsTail, t.nextDep = void 0, j.depsTail.nextDep = t, j.depsTail = t, j.deps === t && (j.deps = e);
		}
		return t;
	}
	trigger(e) {
		this.version++, Ve++, this.notify(e);
	}
	notify(e) {
		ke();
		try {
			for (let e = this.subs; e; e = e.prevSub) e.sub.notify() && e.sub.dep.notify();
		} finally {
			Ae();
		}
	}
};
function We(e) {
	if (e.dep.sc++, e.sub.flags & 4) {
		let t = e.dep.computed;
		if (t && !e.dep.subs) {
			t.flags |= 20;
			for (let e = t.deps; e; e = e.nextDep) We(e);
		}
		let n = e.dep.subs;
		n !== e && (e.prevSub = n, n && (n.nextSub = e)), e.dep.subs = e;
	}
}
var Ge = /* @__PURE__ */ new WeakMap(), Ke = /* @__PURE__ */ Symbol(""), qe = /* @__PURE__ */ Symbol(""), Je = /* @__PURE__ */ Symbol("");
function N(e, t, n) {
	if (M && j) {
		let t = Ge.get(e);
		t || Ge.set(e, t = /* @__PURE__ */ new Map());
		let r = t.get(n);
		r || (t.set(n, r = new Ue()), r.map = t, r.key = n), r.track();
	}
}
function P(e, t, n, r, i, a) {
	let o = Ge.get(e);
	if (!o) {
		Ve++;
		return;
	}
	let s = (e) => {
		e && e.trigger();
	};
	if (ke(), t === "clear") o.forEach(s);
	else {
		let i = d(e), a = i && w(n);
		if (i && n === "length") {
			let e = Number(r);
			o.forEach((t, n) => {
				(n === "length" || n === Je || !_(n) && n >= e) && s(t);
			});
		} else switch ((n !== void 0 || o.has(void 0)) && s(o.get(n)), a && s(o.get(Je)), t) {
			case "add":
				i ? a && s(o.get("length")) : (s(o.get(Ke)), f(e) && s(o.get(qe)));
				break;
			case "delete":
				i || (s(o.get(Ke)), f(e) && s(o.get(qe)));
				break;
			case "set":
				f(e) && s(o.get(Ke));
				break;
		}
	}
	Ae();
}
function Ye(e) {
	let t = /* @__PURE__ */ R(e);
	return t === e ? t : (N(t, "iterate", Je), /* @__PURE__ */ L(e) ? t : t.map(z));
}
function Xe(e) {
	return N(e = /* @__PURE__ */ R(e), "iterate", Je), e;
}
function F(e, t) {
	return /* @__PURE__ */ Mt(e) ? Ft(/* @__PURE__ */ jt(e) ? z(t) : t) : z(t);
}
var Ze = {
	__proto__: null,
	[Symbol.iterator]() {
		return Qe(this, Symbol.iterator, (e) => F(this, e));
	},
	concat(...e) {
		return Ye(this).concat(...e.map((e) => d(e) ? Ye(e) : e));
	},
	entries() {
		return Qe(this, "entries", (e) => (e[1] = F(this, e[1]), e));
	},
	every(e, t) {
		return I(this, "every", e, t, void 0, arguments);
	},
	filter(e, t) {
		return I(this, "filter", e, t, (e) => e.map((e) => F(this, e)), arguments);
	},
	find(e, t) {
		return I(this, "find", e, t, (e) => F(this, e), arguments);
	},
	findIndex(e, t) {
		return I(this, "findIndex", e, t, void 0, arguments);
	},
	findLast(e, t) {
		return I(this, "findLast", e, t, (e) => F(this, e), arguments);
	},
	findLastIndex(e, t) {
		return I(this, "findLastIndex", e, t, void 0, arguments);
	},
	forEach(e, t) {
		return I(this, "forEach", e, t, void 0, arguments);
	},
	includes(...e) {
		return tt(this, "includes", e);
	},
	indexOf(...e) {
		return tt(this, "indexOf", e);
	},
	join(e) {
		return Ye(this).join(e);
	},
	lastIndexOf(...e) {
		return tt(this, "lastIndexOf", e);
	},
	map(e, t) {
		return I(this, "map", e, t, void 0, arguments);
	},
	pop() {
		return nt(this, "pop");
	},
	push(...e) {
		return nt(this, "push", e);
	},
	reduce(e, ...t) {
		return et(this, "reduce", e, t);
	},
	reduceRight(e, ...t) {
		return et(this, "reduceRight", e, t);
	},
	shift() {
		return nt(this, "shift");
	},
	some(e, t) {
		return I(this, "some", e, t, void 0, arguments);
	},
	splice(...e) {
		return nt(this, "splice", e);
	},
	toReversed() {
		return Ye(this).toReversed();
	},
	toSorted(e) {
		return Ye(this).toSorted(e);
	},
	toSpliced(...e) {
		return Ye(this).toSpliced(...e);
	},
	unshift(...e) {
		return nt(this, "unshift", e);
	},
	values() {
		return Qe(this, "values", (e) => F(this, e));
	}
};
function Qe(e, t, n) {
	let r = Xe(e), i = r[t]();
	return r !== e && !/* @__PURE__ */ L(e) && (i._next = i.next, i.next = () => {
		let e = i._next();
		return e.done || (e.value = n(e.value)), e;
	}), i;
}
var $e = Array.prototype;
function I(e, t, n, r, i, a) {
	let o = Xe(e), s = o !== e && !/* @__PURE__ */ L(e), c = o[t];
	if (c !== $e[t]) {
		let t = c.apply(e, a);
		return s ? z(t) : t;
	}
	let l = n;
	o !== e && (s ? l = function(t, r) {
		return n.call(this, F(e, t), r, e);
	} : n.length > 2 && (l = function(t, r) {
		return n.call(this, t, r, e);
	}));
	let u = c.call(o, l, r);
	return s && i ? i(u) : u;
}
function et(e, t, n, r) {
	let i = Xe(e), a = i !== e && !/* @__PURE__ */ L(e), o = n, s = !1;
	i !== e && (a ? (s = r.length === 0, o = function(t, r, i) {
		return s && (s = !1, t = F(e, t)), n.call(this, t, F(e, r), i, e);
	}) : n.length > 3 && (o = function(t, r, i) {
		return n.call(this, t, r, i, e);
	}));
	let c = i[t](o, ...r);
	return s ? F(e, c) : c;
}
function tt(e, t, n) {
	let r = /* @__PURE__ */ R(e);
	N(r, "iterate", Je);
	let i = r[t](...n);
	return (i === -1 || i === !1) && /* @__PURE__ */ Nt(n[0]) ? (n[0] = /* @__PURE__ */ R(n[0]), r[t](...n)) : i;
}
function nt(e, t, n = []) {
	Re(), ke();
	let r = (/* @__PURE__ */ R(e))[t].apply(e, n);
	return Ae(), ze(), r;
}
var rt = /* @__PURE__ */ e("__proto__,__v_isRef,__isVue"), it = new Set(/* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(_));
function at(e) {
	_(e) || (e = String(e));
	let t = /* @__PURE__ */ R(this);
	return N(t, "has", e), t.hasOwnProperty(e);
}
var ot = class {
	constructor(e = !1, t = !1) {
		this._isReadonly = e, this._isShallow = t;
	}
	get(e, t, n) {
		if (t === "__v_skip") return e.__v_skip;
		let r = this._isReadonly, i = this._isShallow;
		if (t === "__v_isReactive") return !r;
		if (t === "__v_isReadonly") return r;
		if (t === "__v_isShallow") return i;
		if (t === "__v_raw") return n === (r ? i ? wt : Ct : i ? St : xt).get(e) || Object.getPrototypeOf(e) === Object.getPrototypeOf(n) ? e : void 0;
		let a = d(e);
		if (!r) {
			let e;
			if (a && (e = Ze[t])) return e;
			if (t === "hasOwnProperty") return at;
		}
		let o = Reflect.get(e, t, /* @__PURE__ */ B(e) ? e : n);
		if ((_(t) ? it.has(t) : rt(t)) || (r || N(e, "get", t), i)) return o;
		if (/* @__PURE__ */ B(o)) {
			let e = a && w(t) ? o : o.value;
			return r && v(e) ? /* @__PURE__ */ kt(e) : e;
		}
		return v(o) ? r ? /* @__PURE__ */ kt(o) : /* @__PURE__ */ Dt(o) : o;
	}
}, st = class extends ot {
	constructor(e = !1) {
		super(!1, e);
	}
	set(e, t, n, r) {
		let i = e[t], a = d(e) && w(t);
		if (!this._isShallow) {
			let e = /* @__PURE__ */ Mt(i);
			if (!/* @__PURE__ */ L(n) && !/* @__PURE__ */ Mt(n) && (i = /* @__PURE__ */ R(i), n = /* @__PURE__ */ R(n)), !a && /* @__PURE__ */ B(i) && !/* @__PURE__ */ B(n)) return e || (i.value = n), !0;
		}
		let o = a ? Number(t) < e.length : u(e, t), s = Reflect.set(e, t, n, /* @__PURE__ */ B(e) ? e : r);
		return e === /* @__PURE__ */ R(r) && (o ? D(n, i) && P(e, "set", t, n, i) : P(e, "add", t, n)), s;
	}
	deleteProperty(e, t) {
		let n = u(e, t), r = e[t], i = Reflect.deleteProperty(e, t);
		return i && n && P(e, "delete", t, void 0, r), i;
	}
	has(e, t) {
		let n = Reflect.has(e, t);
		return (!_(t) || !it.has(t)) && N(e, "has", t), n;
	}
	ownKeys(e) {
		return N(e, "iterate", d(e) ? "length" : Ke), Reflect.ownKeys(e);
	}
}, ct = class extends ot {
	constructor(e = !1) {
		super(!0, e);
	}
	set(e, t) {
		return !0;
	}
	deleteProperty(e, t) {
		return !0;
	}
}, lt = /* @__PURE__ */ new st(), ut = /* @__PURE__ */ new ct(), dt = /* @__PURE__ */ new st(!0), ft = (e) => e, pt = (e) => Reflect.getPrototypeOf(e);
function mt(e, t, n) {
	return function(...r) {
		let i = this.__v_raw, a = /* @__PURE__ */ R(i), o = f(a), c = e === "entries" || e === Symbol.iterator && o, l = e === "keys" && o, u = i[e](...r), d = n ? ft : t ? Ft : z;
		return !t && N(a, "iterate", l ? qe : Ke), s(Object.create(u), { next() {
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
function ht(e) {
	return function(...t) {
		return e === "delete" ? !1 : e === "clear" ? void 0 : this;
	};
}
function gt(e, t) {
	let n = {
		get(n) {
			let r = this.__v_raw, i = /* @__PURE__ */ R(r), a = /* @__PURE__ */ R(n);
			e || (D(n, a) && N(i, "get", n), N(i, "get", a));
			let { has: o } = pt(i), s = t ? ft : e ? Ft : z;
			if (o.call(i, n)) return s(r.get(n));
			if (o.call(i, a)) return s(r.get(a));
			r !== i && r.get(n);
		},
		get size() {
			let t = this.__v_raw;
			return !e && N(/* @__PURE__ */ R(t), "iterate", Ke), t.size;
		},
		has(t) {
			let n = this.__v_raw, r = /* @__PURE__ */ R(n), i = /* @__PURE__ */ R(t);
			return e || (D(t, i) && N(r, "has", t), N(r, "has", i)), t === i ? n.has(t) : n.has(t) || n.has(i);
		},
		forEach(n, r) {
			let i = this, a = i.__v_raw, o = /* @__PURE__ */ R(a), s = t ? ft : e ? Ft : z;
			return !e && N(o, "iterate", Ke), a.forEach((e, t) => n.call(r, s(e), s(t), i));
		}
	};
	return s(n, e ? {
		add: ht("add"),
		set: ht("set"),
		delete: ht("delete"),
		clear: ht("clear")
	} : {
		add(e) {
			let n = /* @__PURE__ */ R(this), r = pt(n), i = /* @__PURE__ */ R(e), a = !t && !/* @__PURE__ */ L(e) && !/* @__PURE__ */ Mt(e) ? i : e;
			return r.has.call(n, a) || D(e, a) && r.has.call(n, e) || D(i, a) && r.has.call(n, i) || (n.add(a), P(n, "add", a, a)), this;
		},
		set(e, n) {
			!t && !/* @__PURE__ */ L(n) && !/* @__PURE__ */ Mt(n) && (n = /* @__PURE__ */ R(n));
			let r = /* @__PURE__ */ R(this), { has: i, get: a } = pt(r), o = i.call(r, e);
			o ||= (e = /* @__PURE__ */ R(e), i.call(r, e));
			let s = a.call(r, e);
			return r.set(e, n), o ? D(n, s) && P(r, "set", e, n, s) : P(r, "add", e, n), this;
		},
		delete(e) {
			let t = /* @__PURE__ */ R(this), { has: n, get: r } = pt(t), i = n.call(t, e);
			i ||= (e = /* @__PURE__ */ R(e), n.call(t, e));
			let a = r ? r.call(t, e) : void 0, o = t.delete(e);
			return i && P(t, "delete", e, void 0, a), o;
		},
		clear() {
			let e = /* @__PURE__ */ R(this), t = e.size !== 0, n = e.clear();
			return t && P(e, "clear", void 0, void 0, void 0), n;
		}
	}), [
		"keys",
		"values",
		"entries",
		Symbol.iterator
	].forEach((r) => {
		n[r] = mt(r, e, t);
	}), n;
}
function _t(e, t) {
	let n = gt(e, t);
	return (t, r, i) => r === "__v_isReactive" ? !e : r === "__v_isReadonly" ? e : r === "__v_raw" ? t : Reflect.get(u(n, r) && r in t ? n : t, r, i);
}
var vt = { get: /* @__PURE__ */ _t(!1, !1) }, yt = { get: /* @__PURE__ */ _t(!1, !0) }, bt = { get: /* @__PURE__ */ _t(!0, !1) }, xt = /* @__PURE__ */ new WeakMap(), St = /* @__PURE__ */ new WeakMap(), Ct = /* @__PURE__ */ new WeakMap(), wt = /* @__PURE__ */ new WeakMap();
function Tt(e) {
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
function Et(e) {
	return e.__v_skip || !Object.isExtensible(e) ? 0 : Tt(S(e));
}
/* @__NO_SIDE_EFFECTS__ */
function Dt(e) {
	return /* @__PURE__ */ Mt(e) ? e : At(e, !1, lt, vt, xt);
}
/* @__NO_SIDE_EFFECTS__ */
function Ot(e) {
	return At(e, !1, dt, yt, St);
}
/* @__NO_SIDE_EFFECTS__ */
function kt(e) {
	return At(e, !0, ut, bt, Ct);
}
function At(e, t, n, r, i) {
	if (!v(e) || e.__v_raw && !(t && e.__v_isReactive)) return e;
	let a = Et(e);
	if (a === 0) return e;
	let o = i.get(e);
	if (o) return o;
	let s = new Proxy(e, a === 2 ? r : n);
	return i.set(e, s), s;
}
/* @__NO_SIDE_EFFECTS__ */
function jt(e) {
	return /* @__PURE__ */ Mt(e) ? /* @__PURE__ */ jt(e.__v_raw) : !!(e && e.__v_isReactive);
}
/* @__NO_SIDE_EFFECTS__ */
function Mt(e) {
	return !!(e && e.__v_isReadonly);
}
/* @__NO_SIDE_EFFECTS__ */
function L(e) {
	return !!(e && e.__v_isShallow);
}
/* @__NO_SIDE_EFFECTS__ */
function Nt(e) {
	return e ? !!e.__v_raw : !1;
}
/* @__NO_SIDE_EFFECTS__ */
function R(e) {
	let t = e && e.__v_raw;
	return t ? /* @__PURE__ */ R(t) : e;
}
function Pt(e) {
	return !u(e, "__v_skip") && Object.isExtensible(e) && O(e, "__v_skip", !0), e;
}
var z = (e) => v(e) ? /* @__PURE__ */ Dt(e) : e, Ft = (e) => v(e) ? /* @__PURE__ */ kt(e) : e;
/* @__NO_SIDE_EFFECTS__ */
function B(e) {
	return e ? e.__v_isRef === !0 : !1;
}
/* @__NO_SIDE_EFFECTS__ */
function It(e) {
	return Lt(e, !1);
}
function Lt(e, t) {
	return /* @__PURE__ */ B(e) ? e : new Rt(e, t);
}
var Rt = class {
	constructor(e, t) {
		this.dep = new Ue(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = t ? e : /* @__PURE__ */ R(e), this._value = t ? e : z(e), this.__v_isShallow = t;
	}
	get value() {
		return this.dep.track(), this._value;
	}
	set value(e) {
		let t = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ L(e) || /* @__PURE__ */ Mt(e);
		e = n ? e : /* @__PURE__ */ R(e), D(e, t) && (this._rawValue = e, this._value = n ? e : z(e), this.dep.trigger());
	}
};
function zt(e) {
	return /* @__PURE__ */ B(e) ? e.value : e;
}
var Bt = {
	get: (e, t, n) => t === "__v_raw" ? e : zt(Reflect.get(e, t, n)),
	set: (e, t, n, r) => {
		let i = e[t];
		return /* @__PURE__ */ B(i) && !/* @__PURE__ */ B(n) ? (i.value = n, !0) : Reflect.set(e, t, n, r);
	}
};
function Vt(e) {
	return /* @__PURE__ */ jt(e) ? e : new Proxy(e, Bt);
}
var Ht = class {
	constructor(e, t, n) {
		this.fn = e, this.setter = t, this._value = void 0, this.dep = new Ue(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = Ve - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !t, this.isSSR = n;
	}
	notify() {
		if (this.flags |= 16, !(this.flags & 8) && j !== this) return Oe(this, !0), !0;
	}
	get value() {
		let e = this.dep.track();
		return Pe(this), e && (e.version = this.dep.version), this._value;
	}
	set value(e) {
		this.setter && this.setter(e);
	}
};
/* @__NO_SIDE_EFFECTS__ */
function Ut(e, t, n = !1) {
	let r, i;
	return h(e) ? r = e : (r = e.get, i = e.set), new Ht(r, i, n);
}
var Wt = {}, Gt = /* @__PURE__ */ new WeakMap(), Kt = void 0;
function qt(e, t = !1, n = Kt) {
	if (n) {
		let t = Gt.get(n);
		t || Gt.set(n, t = []), t.push(e);
	}
}
function Jt(e, n, i = t) {
	let { immediate: a, deep: o, once: s, scheduler: l, augmentJob: u, call: f } = i, p = (e) => o ? e : /* @__PURE__ */ L(e) || o === !1 || o === 0 ? V(e, 1) : V(e), m, g, _, v, y = !1, b = !1;
	if (/* @__PURE__ */ B(e) ? (g = () => e.value, y = /* @__PURE__ */ L(e)) : /* @__PURE__ */ jt(e) ? (g = () => p(e), y = !0) : d(e) ? (b = !0, y = e.some((e) => /* @__PURE__ */ jt(e) || /* @__PURE__ */ L(e)), g = () => e.map((e) => {
		if (/* @__PURE__ */ B(e)) return e.value;
		if (/* @__PURE__ */ jt(e)) return p(e);
		if (h(e)) return f ? f(e, 2) : e();
	})) : g = h(e) ? n ? f ? () => f(e, 2) : e : () => {
		if (_) {
			Re();
			try {
				_();
			} finally {
				ze();
			}
		}
		let t = Kt;
		Kt = m;
		try {
			return f ? f(e, 3, [v]) : e(v);
		} finally {
			Kt = t;
		}
	} : r, n && o) {
		let e = g, t = o === !0 ? Infinity : o;
		g = () => V(e(), t);
	}
	let x = Se(), S = () => {
		m.stop(), x && x.active && c(x.effects, m);
	};
	if (s && n) {
		let e = n;
		n = (...t) => {
			e(...t), S();
		};
	}
	let C = b ? Array(e.length).fill(Wt) : Wt, w = (e) => {
		if (!(!(m.flags & 1) || !m.dirty && !e)) if (n) {
			let e = m.run();
			if (o || y || (b ? e.some((e, t) => D(e, C[t])) : D(e, C))) {
				_ && _();
				let t = Kt;
				Kt = m;
				try {
					let t = [
						e,
						C === Wt ? void 0 : b && C[0] === Wt ? [] : C,
						v
					];
					C = e, f ? f(n, 3, t) : n(...t);
				} finally {
					Kt = t;
				}
			}
		} else m.run();
	};
	return u && u(w), m = new we(g), m.scheduler = l ? () => l(w, !1) : w, v = (e) => qt(e, !1, m), _ = m.onStop = () => {
		let e = Gt.get(m);
		if (e) {
			if (f) f(e, 4);
			else for (let t of e) t();
			Gt.delete(m);
		}
	}, n ? a ? w(!0) : C = m.run() : l ? l(w.bind(null, !0), !0) : m.run(), S.pause = m.pause.bind(m), S.resume = m.resume.bind(m), S.stop = S, S;
}
function V(e, t = Infinity, n) {
	if (t <= 0 || !v(e) || e.__v_skip || (n ||= /* @__PURE__ */ new Map(), (n.get(e) || 0) >= t)) return e;
	if (n.set(e, t), t--, /* @__PURE__ */ B(e)) V(e.value, t, n);
	else if (d(e)) for (let r = 0; r < e.length; r++) V(e[r], t, n);
	else if (p(e) || f(e)) e.forEach((e) => {
		V(e, t, n);
	});
	else if (C(e)) {
		for (let r in e) V(e[r], t, n);
		for (let r of Object.getOwnPropertySymbols(e)) Object.prototype.propertyIsEnumerable.call(e, r) && V(e[r], t, n);
	}
	return e;
}
//#endregion
//#region node_modules/@vue/runtime-core/dist/runtime-core.esm-bundler.js
function Yt(e, t, n, r) {
	try {
		return r ? e(...r) : e();
	} catch (e) {
		Xt(e, t, n);
	}
}
function H(e, t, n, r) {
	if (h(e)) {
		let i = Yt(e, t, n, r);
		return i && y(i) && i.catch((e) => {
			Xt(e, t, n);
		}), i;
	}
	if (d(e)) {
		let i = [];
		for (let a = 0; a < e.length; a++) i.push(H(e[a], t, n, r));
		return i;
	}
}
function Xt(e, n, r, i = !0) {
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
			Re(), Yt(o, null, 10, [
				e,
				i,
				a
			]), ze();
			return;
		}
	}
	Zt(e, r, a, i, s);
}
function Zt(e, t, n, r = !0, i = !1) {
	if (i) throw e;
	console.error(e);
}
var U = [], W = -1, Qt = [], $t = null, en = 0, tn = /* @__PURE__ */ Promise.resolve(), nn = null;
function rn(e) {
	let t = nn || tn;
	return e ? t.then(this ? e.bind(this) : e) : t;
}
function an(e) {
	let t = W + 1, n = U.length;
	for (; t < n;) {
		let r = t + n >>> 1, i = U[r], a = dn(i);
		a < e || a === e && i.flags & 2 ? t = r + 1 : n = r;
	}
	return t;
}
function on(e) {
	if (!(e.flags & 1)) {
		let t = dn(e), n = U[U.length - 1];
		!n || !(e.flags & 2) && t >= dn(n) ? U.push(e) : U.splice(an(t), 0, e), e.flags |= 1, sn();
	}
}
function sn() {
	nn ||= tn.then(fn);
}
function cn(e) {
	d(e) ? Qt.push(...e) : $t && e.id === -1 ? $t.splice(en + 1, 0, e) : e.flags & 1 || (Qt.push(e), e.flags |= 1), sn();
}
function ln(e, t, n = W + 1) {
	for (; n < U.length; n++) {
		let t = U[n];
		if (t && t.flags & 2) {
			if (e && t.id !== e.uid) continue;
			U.splice(n, 1), n--, t.flags & 4 && (t.flags &= -2), t(), t.flags & 4 || (t.flags &= -2);
		}
	}
}
function un(e) {
	if (Qt.length) {
		let e = [...new Set(Qt)].sort((e, t) => dn(e) - dn(t));
		if (Qt.length = 0, $t) {
			$t.push(...e);
			return;
		}
		for ($t = e, en = 0; en < $t.length; en++) {
			let e = $t[en];
			e.flags & 4 && (e.flags &= -2), e.flags & 8 || e(), e.flags &= -2;
		}
		$t = null, en = 0;
	}
}
var dn = (e) => e.id == null ? e.flags & 2 ? -1 : Infinity : e.id;
function fn(e) {
	try {
		for (W = 0; W < U.length; W++) {
			let e = U[W];
			e && !(e.flags & 8) && (e.flags & 4 && (e.flags &= -2), Yt(e, e.i, e.i ? 15 : 14), e.flags & 4 || (e.flags &= -2));
		}
	} finally {
		for (; W < U.length; W++) {
			let e = U[W];
			e && (e.flags &= -2);
		}
		W = -1, U.length = 0, un(e), nn = null, (U.length || Qt.length) && fn(e);
	}
}
var G = null, pn = null;
function mn(e) {
	let t = G;
	return G = e, pn = e && e.type.__scopeId || null, t;
}
function hn(e, t = G, n) {
	if (!t || e._n) return e;
	let r = (...n) => {
		r._d && vi(-1);
		let i = mn(t), a;
		try {
			a = e(...n);
		} finally {
			mn(i), r._d && vi(1);
		}
		return a;
	};
	return r._n = !0, r._c = !0, r._d = !0, r;
}
function gn(e, n) {
	if (G === null) return e;
	let r = $i(G), i = e.dirs ||= [];
	for (let e = 0; e < n.length; e++) {
		let [a, o, s, c = t] = n[e];
		a && (h(a) && (a = {
			mounted: a,
			updated: a
		}), a.deep && V(o), i.push({
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
function _n(e, t, n, r) {
	let i = e.dirs, a = t && t.dirs;
	for (let o = 0; o < i.length; o++) {
		let s = i[o];
		a && (s.oldValue = a[o].value);
		let c = s.dir[r];
		c && (Re(), H(c, n, 8, [
			e.el,
			s,
			e,
			t
		]), ze());
	}
}
function vn(e, t) {
	if ($) {
		let n = $.provides, r = $.parent && $.parent.provides;
		r === n && (n = $.provides = Object.create(r)), n[e] = t;
	}
}
function yn(e, t, n = !1) {
	let r = Ri();
	if (r || Sr) {
		let i = Sr ? Sr._context.provides : r ? r.parent == null || r.ce ? r.vnode.appContext && r.vnode.appContext.provides : r.parent.provides : void 0;
		if (i && e in i) return i[e];
		if (arguments.length > 1) return n && h(t) ? t.call(r && r.proxy) : t;
	}
}
var bn = /* @__PURE__ */ Symbol.for("v-scx"), xn = () => yn(bn);
function Sn(e, t, n) {
	return Cn(e, t, n);
}
function Cn(e, n, i = t) {
	let { immediate: a, deep: o, flush: c, once: l } = i, u = s({}, i), d = n && a || !n && c !== "post", f;
	if (Wi) {
		if (c === "sync") {
			let e = xn();
			f = e.__watcherHandles ||= [];
		} else if (!d) {
			let e = () => {};
			return e.stop = r, e.resume = r, e.pause = r, e;
		}
	}
	let p = $;
	u.call = (e, t, n) => H(e, p, t, n);
	let m = !1;
	c === "post" ? u.scheduler = (e) => {
		q(e, p && p.suspense);
	} : c !== "sync" && (m = !0, u.scheduler = (e, t) => {
		t ? e() : on(e);
	}), u.augmentJob = (e) => {
		n && (e.flags |= 4), m && (e.flags |= 2, p && (e.id = p.uid, e.i = p));
	};
	let h = Jt(e, n, u);
	return Wi && (f ? f.push(h) : d && h()), h;
}
function wn(e, t, n) {
	let r = this.proxy, i = g(e) ? e.includes(".") ? Tn(r, e) : () => r[e] : e.bind(r, r), a;
	h(t) ? a = t : (a = t.handler, n = t);
	let o = Vi(this), s = Cn(i, a.bind(r), n);
	return o(), s;
}
function Tn(e, t) {
	let n = t.split(".");
	return () => {
		let t = e;
		for (let e = 0; e < n.length && t; e++) t = t[n[e]];
		return t;
	};
}
var En = /* @__PURE__ */ Symbol("_vte"), Dn = (e) => e.__isTeleport, On = /* @__PURE__ */ Symbol("_leaveCb");
function kn(e, t) {
	e.shapeFlag & 6 && e.component ? (e.transition = t, kn(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
function An(e) {
	e.ids = [
		e.ids[0] + e.ids[2]++ + "-",
		0,
		0
	];
}
function jn(e, t) {
	let n;
	return !!((n = Object.getOwnPropertyDescriptor(e, t)) && !n.configurable);
}
var Mn = /* @__PURE__ */ new WeakMap();
function Nn(e, n, r, a, o = !1) {
	if (d(e)) {
		e.forEach((e, t) => Nn(e, n && (d(n) ? n[t] : n), r, a, o));
		return;
	}
	if (Fn(a) && !o) {
		a.shapeFlag & 512 && a.type.__asyncResolved && a.component.subTree.component && Nn(e, n, r, a.component.subTree);
		return;
	}
	let s = a.shapeFlag & 4 ? $i(a.component) : a.el, l = o ? null : s, { i: f, r: p } = e, m = n && n.r, _ = f.refs === t ? f.refs = {} : f.refs, v = f.setupState, y = /* @__PURE__ */ R(v), b = v === t ? i : (e) => jn(_, e) ? !1 : u(y, e), x = (e, t) => !(t && jn(_, t));
	if (m != null && m !== p) {
		if (Pn(n), g(m)) _[m] = null, b(m) && (v[m] = null);
		else if (/* @__PURE__ */ B(m)) {
			let e = n;
			x(m, e.k) && (m.value = null), e.k && (_[e.k] = null);
		}
	}
	if (h(p)) Yt(p, f, 12, [l, _]);
	else {
		let t = g(p), n = /* @__PURE__ */ B(p);
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
					i(), Mn.delete(e);
				};
				t.id = -1, Mn.set(e, t), q(t, r);
			} else Pn(e), i();
		}
	}
}
function Pn(e) {
	let t = Mn.get(e);
	t && (t.flags |= 8, Mn.delete(e));
}
le().requestIdleCallback, le().cancelIdleCallback;
var Fn = (e) => !!e.type.__asyncLoader, In = (e) => e.type.__isKeepAlive;
function Ln(e, t) {
	zn(e, "a", t);
}
function Rn(e, t) {
	zn(e, "da", t);
}
function zn(e, t, n = $) {
	let r = e.__wdc ||= () => {
		let t = n;
		for (; t;) {
			if (t.isDeactivated) return;
			t = t.parent;
		}
		return e();
	};
	if (Vn(t, r, n), n) {
		let e = n.parent;
		for (; e && e.parent;) In(e.parent.vnode) && Bn(r, t, n, e), e = e.parent;
	}
}
function Bn(e, t, n, r) {
	let i = Vn(t, e, r, !0);
	Jn(() => {
		c(r[t], i);
	}, n);
}
function Vn(e, t, n = $, r = !1) {
	if (n) {
		let i = n[e] || (n[e] = []), a = t.__weh ||= (...r) => {
			Re();
			let i = Vi(n), a = H(t, n, e, r);
			return i(), ze(), a;
		};
		return r ? i.unshift(a) : i.push(a), a;
	}
}
var Hn = (e) => (t, n = $) => {
	(!Wi || e === "sp") && Vn(e, (...e) => t(...e), n);
}, Un = Hn("bm"), Wn = Hn("m"), Gn = Hn("bu"), Kn = Hn("u"), qn = Hn("bum"), Jn = Hn("um"), Yn = Hn("sp"), Xn = Hn("rtg"), Zn = Hn("rtc");
function Qn(e, t = $) {
	Vn("ec", e, t);
}
var $n = /* @__PURE__ */ Symbol.for("v-ndc"), er = (e) => e ? Ui(e) ? $i(e) : er(e.parent) : null, tr = /* @__PURE__ */ s(/* @__PURE__ */ Object.create(null), {
	$: (e) => e,
	$el: (e) => e.vnode.el,
	$data: (e) => e.data,
	$props: (e) => e.props,
	$attrs: (e) => e.attrs,
	$slots: (e) => e.slots,
	$refs: (e) => e.refs,
	$parent: (e) => er(e.parent),
	$root: (e) => er(e.root),
	$host: (e) => e.ce,
	$emit: (e) => e.emit,
	$options: (e) => ur(e),
	$forceUpdate: (e) => e.f ||= () => {
		on(e.update);
	},
	$nextTick: (e) => e.n ||= rn.bind(e.proxy),
	$watch: (e) => wn.bind(e)
}), nr = (e, n) => e !== t && !e.__isScriptSetup && u(e, n), rr = {
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
			else if (nr(i, n)) return s[n] = 1, i[n];
			else if (a !== t && u(a, n)) return s[n] = 2, a[n];
			else if (u(o, n)) return s[n] = 3, o[n];
			else if (r !== t && u(r, n)) return s[n] = 4, r[n];
			else ar && (s[n] = 0);
		}
		let d = tr[n], f, p;
		if (d) return n === "$attrs" && N(e.attrs, "get", ""), d(e);
		if ((f = c.__cssModules) && (f = f[n])) return f;
		if (r !== t && u(r, n)) return s[n] = 4, r[n];
		if (p = l.config.globalProperties, u(p, n)) return p[n];
	},
	set({ _: e }, n, r) {
		let { data: i, setupState: a, ctx: o } = e;
		return nr(a, n) ? (a[n] = r, !0) : i !== t && u(i, n) ? (i[n] = r, !0) : u(e.props, n) || n[0] === "$" && n.slice(1) in e ? !1 : (o[n] = r, !0);
	},
	has({ _: { data: e, setupState: n, accessCache: r, ctx: i, appContext: a, props: o, type: s } }, c) {
		let l;
		return !!(r[c] || e !== t && c[0] !== "$" && u(e, c) || nr(n, c) || u(o, c) || u(i, c) || u(tr, c) || u(a.config.globalProperties, c) || (l = s.__cssModules) && l[c]);
	},
	defineProperty(e, t, n) {
		return n.get == null ? u(n, "value") && this.set(e, t, n.value, null) : e._.accessCache[t] = 0, Reflect.defineProperty(e, t, n);
	}
};
function ir(e) {
	return d(e) ? e.reduce((e, t) => (e[t] = null, e), {}) : e;
}
var ar = !0;
function or(e) {
	let t = ur(e), n = e.proxy, i = e.ctx;
	ar = !1, t.beforeCreate && cr(t.beforeCreate, e, "bc");
	let { data: a, computed: o, methods: s, watch: c, provide: l, inject: u, created: f, beforeMount: p, mounted: m, beforeUpdate: g, updated: _, activated: y, deactivated: b, beforeDestroy: x, beforeUnmount: S, destroyed: C, unmounted: w, render: ee, renderTracked: te, renderTriggered: ne, errorCaptured: T, serverPrefetch: re, expose: E, inheritAttrs: ie, components: ae, directives: D, filters: oe } = t;
	if (u && sr(u, i, null), s) for (let e in s) {
		let t = s[e];
		h(t) && (i[e] = t.bind(n));
	}
	if (a) {
		let t = a.call(n, n);
		v(t) && (e.data = /* @__PURE__ */ Dt(t));
	}
	if (ar = !0, o) for (let e in o) {
		let t = o[e], a = ta({
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
	if (c) for (let e in c) lr(c[e], i, n, e);
	if (l) {
		let e = h(l) ? l.call(n) : l;
		Reflect.ownKeys(e).forEach((t) => {
			vn(t, e[t]);
		});
	}
	f && cr(f, e, "c");
	function O(e, t) {
		d(t) ? t.forEach((t) => e(t.bind(n))) : t && e(t.bind(n));
	}
	if (O(Un, p), O(Wn, m), O(Gn, g), O(Kn, _), O(Ln, y), O(Rn, b), O(Qn, T), O(Zn, te), O(Xn, ne), O(qn, S), O(Jn, w), O(Yn, re), d(E)) if (E.length) {
		let t = e.exposed ||= {};
		E.forEach((e) => {
			Object.defineProperty(t, e, {
				get: () => n[e],
				set: (t) => n[e] = t,
				enumerable: !0
			});
		});
	} else e.exposed ||= {};
	ee && e.render === r && (e.render = ee), ie != null && (e.inheritAttrs = ie), ae && (e.components = ae), D && (e.directives = D), re && An(e);
}
function sr(e, t, n = r) {
	d(e) && (e = hr(e));
	for (let n in e) {
		let r = e[n], i;
		i = v(r) ? "default" in r ? yn(r.from || n, r.default, !0) : yn(r.from || n) : yn(r), /* @__PURE__ */ B(i) ? Object.defineProperty(t, n, {
			enumerable: !0,
			configurable: !0,
			get: () => i.value,
			set: (e) => i.value = e
		}) : t[n] = i;
	}
}
function cr(e, t, n) {
	H(d(e) ? e.map((e) => e.bind(t.proxy)) : e.bind(t.proxy), t, n);
}
function lr(e, t, n, r) {
	let i = r.includes(".") ? Tn(n, r) : () => n[r];
	if (g(e)) {
		let n = t[e];
		h(n) && Sn(i, n);
	} else if (h(e)) Sn(i, e.bind(n));
	else if (v(e)) if (d(e)) e.forEach((e) => lr(e, t, n, r));
	else {
		let r = h(e.handler) ? e.handler.bind(n) : t[e.handler];
		h(r) && Sn(i, r, e);
	}
}
function ur(e) {
	let t = e.type, { mixins: n, extends: r } = t, { mixins: i, optionsCache: a, config: { optionMergeStrategies: o } } = e.appContext, s = a.get(t), c;
	return s ? c = s : !i.length && !n && !r ? c = t : (c = {}, i.length && i.forEach((e) => dr(c, e, o, !0)), dr(c, t, o)), v(t) && a.set(t, c), c;
}
function dr(e, t, n, r = !1) {
	let { mixins: i, extends: a } = t;
	a && dr(e, a, n, !0), i && i.forEach((t) => dr(e, t, n, !0));
	for (let i in t) if (!(r && i === "expose")) {
		let r = fr[i] || n && n[i];
		e[i] = r ? r(e[i], t[i]) : t[i];
	}
	return e;
}
var fr = {
	data: pr,
	props: _r,
	emits: _r,
	methods: gr,
	computed: gr,
	beforeCreate: K,
	created: K,
	beforeMount: K,
	mounted: K,
	beforeUpdate: K,
	updated: K,
	beforeDestroy: K,
	beforeUnmount: K,
	destroyed: K,
	unmounted: K,
	activated: K,
	deactivated: K,
	errorCaptured: K,
	serverPrefetch: K,
	components: gr,
	directives: gr,
	watch: vr,
	provide: pr,
	inject: mr
};
function pr(e, t) {
	return t ? e ? function() {
		return s(h(e) ? e.call(this, this) : e, h(t) ? t.call(this, this) : t);
	} : t : e;
}
function mr(e, t) {
	return gr(hr(e), hr(t));
}
function hr(e) {
	if (d(e)) {
		let t = {};
		for (let n = 0; n < e.length; n++) t[e[n]] = e[n];
		return t;
	}
	return e;
}
function K(e, t) {
	return e ? [...new Set([].concat(e, t))] : t;
}
function gr(e, t) {
	return e ? s(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function _r(e, t) {
	return e ? d(e) && d(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : s(/* @__PURE__ */ Object.create(null), ir(e), ir(t ?? {})) : t;
}
function vr(e, t) {
	if (!e) return t;
	if (!t) return e;
	let n = s(/* @__PURE__ */ Object.create(null), e);
	for (let r in t) n[r] = K(e[r], t[r]);
	return n;
}
function yr() {
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
var br = 0;
function xr(e, t) {
	return function(n, r = null) {
		h(n) || (n = s({}, n)), r != null && !v(r) && (r = null);
		let i = yr(), a = /* @__PURE__ */ new WeakSet(), o = [], c = !1, l = i.app = {
			_uid: br++,
			_component: n,
			_props: r,
			_container: null,
			_context: i,
			_instance: null,
			version: na,
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
					let u = l._ceVNode || Ei(n, r);
					return u.appContext = i, s === !0 ? s = "svg" : s === !1 && (s = void 0), o && t ? t(u, a) : e(u, a, s), c = !0, l._container = a, a.__vue_app__ = l, $i(u.component);
				}
			},
			onUnmount(e) {
				o.push(e);
			},
			unmount() {
				c && (H(o, l._instance, 16), e(null, l._container), delete l._container.__vue_app__);
			},
			provide(e, t) {
				return i.provides[e] = t, l;
			},
			runWithContext(e) {
				let t = Sr;
				Sr = l;
				try {
					return e();
				} finally {
					Sr = t;
				}
			}
		};
		return l;
	};
}
var Sr = null, Cr = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${T(t)}Modifiers`] || e[`${E(t)}Modifiers`];
function wr(e, n, ...r) {
	if (e.isUnmounted) return;
	let i = e.vnode.props || t, a = r, o = n.startsWith("update:"), s = o && Cr(i, n.slice(7));
	s && (s.trim && (a = r.map((e) => g(e) ? e.trim() : e)), s.number && (a = r.map(se)));
	let c, l = i[c = ae(n)] || i[c = ae(T(n))];
	!l && o && (l = i[c = ae(E(n))]), l && H(l, e, 6, a);
	let u = i[c + "Once"];
	if (u) {
		if (!e.emitted) e.emitted = {};
		else if (e.emitted[c]) return;
		e.emitted[c] = !0, H(u, e, 6, a);
	}
}
var Tr = /* @__PURE__ */ new WeakMap();
function Er(e, t, n = !1) {
	let r = n ? Tr : t.emitsCache, i = r.get(e);
	if (i !== void 0) return i;
	let a = e.emits, o = {}, c = !1;
	if (!h(e)) {
		let r = (e) => {
			let n = Er(e, t, !0);
			n && (c = !0, s(o, n));
		};
		!n && t.mixins.length && t.mixins.forEach(r), e.extends && r(e.extends), e.mixins && e.mixins.forEach(r);
	}
	return !a && !c ? (v(e) && r.set(e, null), null) : (d(a) ? a.forEach((e) => o[e] = null) : s(o, a), v(e) && r.set(e, o), o);
}
function Dr(e, t) {
	return !e || !a(t) ? !1 : (t = t.slice(2).replace(/Once$/, ""), u(e, t[0].toLowerCase() + t.slice(1)) || u(e, E(t)) || u(e, t));
}
function Or(e) {
	let { type: t, vnode: n, proxy: r, withProxy: i, propsOptions: [a], slots: s, attrs: c, emit: l, render: u, renderCache: d, props: f, data: p, setupState: m, ctx: h, inheritAttrs: g } = e, _ = mn(e), v, y;
	try {
		if (n.shapeFlag & 4) {
			let e = i || r, t = e;
			v = Z(u.call(t, e, d, f, m, p, h)), y = c;
		} else {
			let e = t;
			v = Z(e.length > 1 ? e(f, {
				attrs: c,
				slots: s,
				emit: l
			}) : e(f, null)), y = t.props ? c : kr(c);
		}
	} catch (t) {
		hi.length = 0, Xt(t, e, 1), v = Ei(pi);
	}
	let b = v;
	if (y && g !== !1) {
		let e = Object.keys(y), { shapeFlag: t } = b;
		e.length && t & 7 && (a && e.some(o) && (y = Ar(y, a)), b = ki(b, y, !1, !0));
	}
	return n.dirs && (b = ki(b, null, !1, !0), b.dirs = b.dirs ? b.dirs.concat(n.dirs) : n.dirs), n.transition && kn(b, n.transition), v = b, mn(_), v;
}
var kr = (e) => {
	let t;
	for (let n in e) (n === "class" || n === "style" || a(n)) && ((t ||= {})[n] = e[n]);
	return t;
}, Ar = (e, t) => {
	let n = {};
	for (let r in e) (!o(r) || !(r.slice(9) in t)) && (n[r] = e[r]);
	return n;
};
function jr(e, t, n) {
	let { props: r, children: i, component: a } = e, { props: o, children: s, patchFlag: c } = t, l = a.emitsOptions;
	if (t.dirs || t.transition) return !0;
	if (n && c >= 0) {
		if (c & 1024) return !0;
		if (c & 16) return r ? Mr(r, o, l) : !!o;
		if (c & 8) {
			let e = t.dynamicProps;
			for (let t = 0; t < e.length; t++) {
				let n = e[t];
				if (Nr(o, r, n) && !Dr(l, n)) return !0;
			}
		}
	} else return (i || s) && (!s || !s.$stable) ? !0 : r === o ? !1 : r ? o ? Mr(r, o, l) : !0 : !!o;
	return !1;
}
function Mr(e, t, n) {
	let r = Object.keys(t);
	if (r.length !== Object.keys(e).length) return !0;
	for (let i = 0; i < r.length; i++) {
		let a = r[i];
		if (Nr(t, e, a) && !Dr(n, a)) return !0;
	}
	return !1;
}
function Nr(e, t, n) {
	let r = e[n], i = t[n];
	return n === "style" && v(r) && v(i) ? !ye(r, i) : r !== i;
}
function Pr({ vnode: e, parent: t, suspense: n }, r) {
	for (; t;) {
		let n = t.subTree;
		if (n.suspense && n.suspense.activeBranch === e && (n.suspense.vnode.el = n.el = r, e = n), n === e) (e = t.vnode).el = r, t = t.parent;
		else break;
	}
	n && n.activeBranch === e && (n.vnode.el = r);
}
var Fr = {}, Ir = () => Object.create(Fr), Lr = (e) => Object.getPrototypeOf(e) === Fr;
function Rr(e, t, n, r = !1) {
	let i = {}, a = Ir();
	e.propsDefaults = /* @__PURE__ */ Object.create(null), Br(e, t, i, a);
	for (let t in e.propsOptions[0]) t in i || (i[t] = void 0);
	n ? e.props = r ? i : /* @__PURE__ */ Ot(i) : e.type.props ? e.props = i : e.props = a, e.attrs = a;
}
function zr(e, t, n, r) {
	let { props: i, attrs: a, vnode: { patchFlag: o } } = e, s = /* @__PURE__ */ R(i), [c] = e.propsOptions, l = !1;
	if ((r || o > 0) && !(o & 16)) {
		if (o & 8) {
			let n = e.vnode.dynamicProps;
			for (let r = 0; r < n.length; r++) {
				let o = n[r];
				if (Dr(e.emitsOptions, o)) continue;
				let d = t[o];
				if (c) if (u(a, o)) d !== a[o] && (a[o] = d, l = !0);
				else {
					let t = T(o);
					i[t] = Vr(c, s, t, d, e, !1);
				}
				else d !== a[o] && (a[o] = d, l = !0);
			}
		}
	} else {
		Br(e, t, i, a) && (l = !0);
		let r;
		for (let a in s) (!t || !u(t, a) && ((r = E(a)) === a || !u(t, r))) && (c ? n && (n[a] !== void 0 || n[r] !== void 0) && (i[a] = Vr(c, s, a, void 0, e, !0)) : delete i[a]);
		if (a !== s) for (let e in a) (!t || !u(t, e)) && (delete a[e], l = !0);
	}
	l && P(e.attrs, "set", "");
}
function Br(e, n, r, i) {
	let [a, o] = e.propsOptions, s = !1, c;
	if (n) for (let t in n) {
		if (ee(t)) continue;
		let l = n[t], d;
		a && u(a, d = T(t)) ? !o || !o.includes(d) ? r[d] = l : (c ||= {})[d] = l : Dr(e.emitsOptions, t) || (!(t in i) || l !== i[t]) && (i[t] = l, s = !0);
	}
	if (o) {
		let n = /* @__PURE__ */ R(r), i = c || t;
		for (let t = 0; t < o.length; t++) {
			let s = o[t];
			r[s] = Vr(a, n, s, i[s], e, !u(i, s));
		}
	}
	return s;
}
function Vr(e, t, n, r, i, a) {
	let o = e[n];
	if (o != null) {
		let e = u(o, "default");
		if (e && r === void 0) {
			let e = o.default;
			if (o.type !== Function && !o.skipFactory && h(e)) {
				let { propsDefaults: a } = i;
				if (n in a) r = a[n];
				else {
					let o = Vi(i);
					r = a[n] = e.call(null, t), o();
				}
			} else r = e;
			i.ce && i.ce._setProp(n, r);
		}
		o[0] && (a && !e ? r = !1 : o[1] && (r === "" || r === E(n)) && (r = !0));
	}
	return r;
}
var Hr = /* @__PURE__ */ new WeakMap();
function Ur(e, r, i = !1) {
	let a = i ? Hr : r.propsCache, o = a.get(e);
	if (o) return o;
	let c = e.props, l = {}, f = [], p = !1;
	if (!h(e)) {
		let t = (e) => {
			p = !0;
			let [t, n] = Ur(e, r, !0);
			s(l, t), n && f.push(...n);
		};
		!i && r.mixins.length && r.mixins.forEach(t), e.extends && t(e.extends), e.mixins && e.mixins.forEach(t);
	}
	if (!c && !p) return v(e) && a.set(e, n), n;
	if (d(c)) for (let e = 0; e < c.length; e++) {
		let n = T(c[e]);
		Wr(n) && (l[n] = t);
	}
	else if (c) for (let e in c) {
		let t = T(e);
		if (Wr(t)) {
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
function Wr(e) {
	return e[0] !== "$" && !ee(e);
}
var Gr = (e) => e === "_" || e === "_ctx" || e === "$stable", Kr = (e) => d(e) ? e.map(Z) : [Z(e)], qr = (e, t, n) => {
	if (t._n) return t;
	let r = hn((...e) => Kr(t(...e)), n);
	return r._c = !1, r;
}, Jr = (e, t, n) => {
	let r = e._ctx;
	for (let n in e) {
		if (Gr(n)) continue;
		let i = e[n];
		if (h(i)) t[n] = qr(n, i, r);
		else if (i != null) {
			let e = Kr(i);
			t[n] = () => e;
		}
	}
}, Yr = (e, t) => {
	let n = Kr(t);
	e.slots.default = () => n;
}, Xr = (e, t, n) => {
	for (let r in t) (n || !Gr(r)) && (e[r] = t[r]);
}, Zr = (e, t, n) => {
	let r = e.slots = Ir();
	if (e.vnode.shapeFlag & 32) {
		let e = t._;
		e ? (Xr(r, t, n), n && O(r, "_", e, !0)) : Jr(t, r);
	} else t && Yr(e, t);
}, Qr = (e, n, r) => {
	let { vnode: i, slots: a } = e, o = !0, s = t;
	if (i.shapeFlag & 32) {
		let e = n._;
		e ? r && e === 1 ? o = !1 : Xr(a, n, r) : (o = !n.$stable, Jr(n, a)), s = n;
	} else n && (Yr(e, n), s = { default: 1 });
	if (o) for (let e in a) !Gr(e) && s[e] == null && delete a[e];
}, q = ui;
function $r(e) {
	return ei(e);
}
function ei(e, i) {
	let a = le();
	a.__VUE__ = !0;
	let { insert: o, remove: s, patchProp: c, createElement: l, createText: u, createComment: d, setText: f, setElementText: p, parentNode: m, nextSibling: h, setScopeId: g = r, insertStaticContent: _ } = e, v = (e, t, n, r = null, i = null, a = null, o = void 0, s = null, c = !!t.dynamicChildren) => {
		if (e === t) return;
		e && !Ci(e, t) && (r = ye(e), k(e, i, a, !0), e = null), t.patchFlag === -2 && (c = !1, t.dynamicChildren = null);
		let { type: l, ref: u, shapeFlag: d } = t;
		switch (l) {
			case fi:
				y(e, t, n, r);
				break;
			case pi:
				b(e, t, n, r);
				break;
			case mi:
				e ?? x(t, n, r, o);
				break;
			case di:
				ae(e, t, n, r, i, a, o, s, c);
				break;
			default: d & 1 ? w(e, t, n, r, i, a, o, s, c) : d & 6 ? D(e, t, n, r, i, a, o, s, c) : (d & 64 || d & 128) && l.process(e, t, n, r, i, a, o, s, c, xe);
		}
		u != null && i ? Nn(u, e && e.ref, a, t || e, !t) : u == null && e && e.ref != null && Nn(e.ref, null, a, e, !0);
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
		if (d = e.el = l(e.type, a, m && m.is, m), h & 8 ? p(d, e.children) : h & 16 && T(e.children, d, null, r, i, ti(e, a), s, u), _ && _n(e, null, r, "created"), ne(d, e, e.scopeId, s, r), m) {
			for (let e in m) e !== "value" && !ee(e) && c(d, e, null, m[e], a, r);
			"value" in m && c(d, "value", null, m.value, a), (f = m.onVnodeBeforeMount) && Q(f, r, e);
		}
		_ && _n(e, null, r, "beforeMount");
		let v = ri(i, g);
		v && g.beforeEnter(d), o(d, t, n), ((f = m && m.onVnodeMounted) || v || _) && q(() => {
			try {
				f && Q(f, r, e), v && g.enter(d), _ && _n(e, null, r, "mounted");
			} finally {}
		}, i);
	}, ne = (e, t, n, r, i) => {
		if (n && g(e, n), r) for (let t = 0; t < r.length; t++) g(e, r[t]);
		if (i) {
			let n = i.subTree;
			if (t === n || li(n.type) && (n.ssContent === t || n.ssFallback === t)) {
				let t = i.vnode;
				ne(e, t, t.scopeId, t.slotScopeIds, i.parent);
			}
		}
	}, T = (e, t, n, r, i, a, o, s, c = 0) => {
		for (let l = c; l < e.length; l++) v(null, e[l] = s ? Mi(e[l]) : Z(e[l]), t, n, r, i, a, o, s);
	}, re = (e, n, r, i, a, o, s) => {
		let l = n.el = e.el, { patchFlag: u, dynamicChildren: d, dirs: f } = n;
		u |= e.patchFlag & 16;
		let m = e.props || t, h = n.props || t, g;
		if (r && ni(r, !1), (g = h.onVnodeBeforeUpdate) && Q(g, r, n, e), f && _n(n, e, r, "beforeUpdate"), r && ni(r, !0), (m.innerHTML && h.innerHTML == null || m.textContent && h.textContent == null) && p(l, ""), d ? E(e.dynamicChildren, d, l, r, i, ti(n, a), o) : s || de(e, n, l, null, r, i, ti(n, a), o, !1), u > 0) {
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
		((g = h.onVnodeUpdated) || f) && q(() => {
			g && Q(g, r, n, e), f && _n(n, e, r, "updated");
		}, i);
	}, E = (e, t, n, r, i, a, o) => {
		for (let s = 0; s < t.length; s++) {
			let c = e[s], l = t[s];
			v(c, l, c.el && (c.type === di || !Ci(c, l) || c.shapeFlag & 198) ? m(c.el) : n, null, r, i, a, o, !0);
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
		h && (c = c ? c.concat(h) : h), e == null ? (o(d, n, r), o(f, n, r), T(t.children || [], n, f, i, a, s, c, l)) : p > 0 && p & 64 && m && e.dynamicChildren && e.dynamicChildren.length === m.length ? (E(e.dynamicChildren, m, n, i, a, s, c), (t.key != null || i && t === i.subTree) && ii(e, t, !0)) : de(e, t, n, f, i, a, s, c, l);
	}, D = (e, t, n, r, i, a, o, s, c) => {
		t.slotScopeIds = s, e == null ? t.shapeFlag & 512 ? i.ctx.activate(t, n, r, o, c) : O(t, n, r, i, a, o, c) : se(e, t, c);
	}, O = (e, t, n, r, i, a, o) => {
		let s = e.component = Li(e, r, i);
		if (In(e) && (s.ctx.renderer = xe), Gi(s, !1, o), s.asyncDep) {
			if (i && i.registerDep(s, ce, o), !e.el) {
				let r = s.subTree = Ei(pi);
				b(null, r, t, n), e.placeholder = r.el;
			}
		} else ce(s, e, t, n, i, a, o);
	}, se = (e, t, n) => {
		let r = t.component = e.component;
		if (jr(e, t, n)) if (r.asyncDep && !r.asyncResolved) {
			ue(r, t, n);
			return;
		} else r.next = t, r.update();
		else t.el = e.el, r.vnode = t;
	}, ce = (e, t, n, r, i, a, o) => {
		let s = () => {
			if (e.isMounted) {
				let { next: t, bu: n, u: r, parent: s, vnode: c } = e;
				{
					let n = oi(e);
					if (n) {
						t && (t.el = c.el, ue(e, t, o)), n.asyncDep.then(() => {
							q(() => {
								e.isUnmounted || l();
							}, i);
						});
						return;
					}
				}
				let u = t, d;
				ni(e, !1), t ? (t.el = c.el, ue(e, t, o)) : t = c, n && oe(n), (d = t.props && t.props.onVnodeBeforeUpdate) && Q(d, s, t, c), ni(e, !0);
				let f = Or(e), p = e.subTree;
				e.subTree = f, v(p, f, m(p.el), ye(p), e, i, a), t.el = f.el, u === null && Pr(e, f.el), r && q(r, i), (d = t.props && t.props.onVnodeUpdated) && q(() => Q(d, s, t, c), i);
			} else {
				let o, { el: s, props: c } = t, { bm: l, m: u, parent: d, root: f, type: p } = e, m = Fn(t);
				if (ni(e, !1), l && oe(l), !m && (o = c && c.onVnodeBeforeMount) && Q(o, d, t), ni(e, !0), s && j) {
					let t = () => {
						e.subTree = Or(e), j(s, e.subTree, e, i, null);
					};
					m && p.__asyncHydrate ? p.__asyncHydrate(s, e, t) : t();
				} else {
					f.ce && f.ce._hasShadowRoot() && f.ce._injectChildStyle(p, e.parent ? e.parent.type : void 0);
					let o = e.subTree = Or(e);
					v(null, o, n, r, e, i, a), t.el = o.el;
				}
				if (u && q(u, i), !m && (o = c && c.onVnodeMounted)) {
					let e = t;
					q(() => Q(o, d, e), i);
				}
				(t.shapeFlag & 256 || d && Fn(d.vnode) && d.vnode.shapeFlag & 256) && e.a && q(e.a, i), e.isMounted = !0, t = n = r = null;
			}
		};
		e.scope.on();
		let c = e.effect = new we(s);
		e.scope.off();
		let l = e.update = c.run.bind(c), u = e.job = c.runIfDirty.bind(c);
		u.i = e, u.id = e.uid, c.scheduler = () => on(u), ni(e, !0), l();
	}, ue = (e, t, n) => {
		t.component = e;
		let r = e.vnode.props;
		e.vnode = t, e.next = null, zr(e, t.props, r, n), Qr(e, t.children, n), Re(), ln(e), ze();
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
			let n = t[p] = l ? Mi(t[p]) : Z(t[p]);
			v(e[p], n, r, null, a, o, s, c, l);
		}
		u > d ? ve(e, a, o, !0, !1, f) : T(t, r, i, a, o, s, c, l, f);
	}, pe = (e, t, r, i, a, o, s, c, l) => {
		let u = 0, d = t.length, f = e.length - 1, p = d - 1;
		for (; u <= f && u <= p;) {
			let n = e[u], i = t[u] = l ? Mi(t[u]) : Z(t[u]);
			if (Ci(n, i)) v(n, i, r, null, a, o, s, c, l);
			else break;
			u++;
		}
		for (; u <= f && u <= p;) {
			let n = e[f], i = t[p] = l ? Mi(t[p]) : Z(t[p]);
			if (Ci(n, i)) v(n, i, r, null, a, o, s, c, l);
			else break;
			f--, p--;
		}
		if (u > f) {
			if (u <= p) {
				let e = p + 1, n = e < d ? t[e].el : i;
				for (; u <= p;) v(null, t[u] = l ? Mi(t[u]) : Z(t[u]), r, n, a, o, s, c, l), u++;
			}
		} else if (u > p) for (; u <= f;) k(e[u], a, o, !0), u++;
		else {
			let m = u, h = u, g = /* @__PURE__ */ new Map();
			for (u = h; u <= p; u++) {
				let e = t[u] = l ? Mi(t[u]) : Z(t[u]);
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
				else for (_ = h; _ <= p; _++) if (C[_ - h] === 0 && Ci(n, t[_])) {
					i = _;
					break;
				}
				i === void 0 ? k(n, a, o, !0) : (C[i - h] = u + 1, i >= S ? S = i : x = !0, v(n, t[i], r, null, a, o, s, c, l), y++);
			}
			let w = x ? ai(C) : n;
			for (_ = w.length - 1, u = b - 1; u >= 0; u--) {
				let e = h + u, n = t[e], f = t[e + 1], p = e + 1 < d ? f.el || ci(f) : i;
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
			c.move(e, t, n, xe);
			return;
		}
		if (c === di) {
			o(a, t, n);
			for (let e = 0; e < u.length; e++) me(u[e], t, n, r);
			o(e.anchor, t, n);
			return;
		}
		if (c === mi) {
			S(e, t, n);
			return;
		}
		if (r !== 2 && d & 1 && l) if (r === 0) l.beforeEnter(a), o(a, t, n), q(() => l.enter(a), i);
		else {
			let { leave: r, delayLeave: i, afterLeave: c } = l, u = () => {
				e.ctx.isUnmounted ? s(a) : o(a, t, n);
			}, d = () => {
				a._isLeaving && a[On](!0), r(a, () => {
					u(), c && c();
				});
			};
			i ? i(a, u, d) : d();
		}
		else o(a, t, n);
	}, k = (e, t, n, r = !1, i = !1) => {
		let { type: a, props: o, ref: s, children: c, dynamicChildren: l, shapeFlag: u, patchFlag: d, dirs: f, cacheIndex: p, memo: m } = e;
		if (d === -2 && (i = !1), s != null && (Re(), Nn(s, null, n, e, !0), ze()), p != null && (t.renderCache[p] = void 0), u & 256) {
			t.ctx.deactivate(e);
			return;
		}
		let h = u & 1 && f, g = !Fn(e), _;
		if (g && (_ = o && o.onVnodeBeforeUnmount) && Q(_, t, e), u & 6) _e(e.component, n, r);
		else {
			if (u & 128) {
				e.suspense.unmount(n, r);
				return;
			}
			h && _n(e, null, t, "beforeUnmount"), u & 64 ? e.type.remove(e, t, n, xe, r) : l && !l.hasOnce && (a !== di || d > 0 && d & 64) ? ve(l, t, n, !1, !0) : (a === di && d & 384 || !i && u & 16) && ve(c, t, n), r && he(e);
		}
		let v = m != null && p == null;
		(g && (_ = o && o.onVnodeUnmounted) || h || v) && q(() => {
			_ && Q(_, t, e), h && _n(e, null, t, "unmounted"), v && (e.el = null);
		}, n);
	}, he = (e) => {
		let { type: t, el: n, anchor: r, transition: i } = e;
		if (t === di) {
			ge(n, r);
			return;
		}
		if (t === mi) {
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
		si(c), si(l), r && oe(r), i.stop(), a && (a.flags |= 8, k(o, e, t, n)), s && q(s, t), q(() => {
			e.isUnmounted = !0;
		}, t);
	}, ve = (e, t, n, r = !1, i = !1, a = 0) => {
		for (let o = a; o < e.length; o++) k(e[o], t, n, r, i);
	}, ye = (e) => {
		if (e.shapeFlag & 6) return ye(e.component.subTree);
		if (e.shapeFlag & 128) return e.suspense.next();
		let t = h(e.anchor || e.el), n = t && t[En];
		return n ? h(n) : t;
	}, be = !1, A = (e, t, n) => {
		let r;
		e == null ? t._vnode && (k(t._vnode, null, null, !0), r = t._vnode.component) : v(t._vnode || null, e, t, null, null, null, n), t._vnode = e, be ||= (be = !0, ln(r), un(), !1);
	}, xe = {
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
	}, Se, j;
	return i && ([Se, j] = i(xe)), {
		render: A,
		hydrate: Se,
		createApp: xr(A, Se)
	};
}
function ti({ type: e, props: t }, n) {
	return n === "svg" && e === "foreignObject" || n === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : n;
}
function ni({ effect: e, job: t }, n) {
	n ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function ri(e, t) {
	return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function ii(e, t, n = !1) {
	let r = e.children, i = t.children;
	if (d(r) && d(i)) for (let e = 0; e < r.length; e++) {
		let t = r[e], a = i[e];
		a.shapeFlag & 1 && !a.dynamicChildren && ((a.patchFlag <= 0 || a.patchFlag === 32) && (a = i[e] = Mi(i[e]), a.el = t.el), !n && a.patchFlag !== -2 && ii(t, a)), a.type === fi && (a.patchFlag === -1 && (a = i[e] = Mi(a)), a.el = t.el), a.type === pi && !a.el && (a.el = t.el);
	}
}
function ai(e) {
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
function oi(e) {
	let t = e.subTree.component;
	if (t) return t.asyncDep && !t.asyncResolved ? t : oi(t);
}
function si(e) {
	if (e) for (let t = 0; t < e.length; t++) e[t].flags |= 8;
}
function ci(e) {
	if (e.placeholder) return e.placeholder;
	let t = e.component;
	return t ? ci(t.subTree) : null;
}
var li = (e) => e.__isSuspense;
function ui(e, t) {
	t && t.pendingBranch ? d(e) ? t.effects.push(...e) : t.effects.push(e) : cn(e);
}
var di = /* @__PURE__ */ Symbol.for("v-fgt"), fi = /* @__PURE__ */ Symbol.for("v-txt"), pi = /* @__PURE__ */ Symbol.for("v-cmt"), mi = /* @__PURE__ */ Symbol.for("v-stc"), hi = [], J = null;
function Y(e = !1) {
	hi.push(J = e ? null : []);
}
function gi() {
	hi.pop(), J = hi[hi.length - 1] || null;
}
var _i = 1;
function vi(e, t = !1) {
	_i += e, e < 0 && J && t && (J.hasOnce = !0);
}
function yi(e) {
	return e.dynamicChildren = _i > 0 ? J || n : null, gi(), _i > 0 && J && J.push(e), e;
}
function bi(e, t, n, r, i, a) {
	return yi(X(e, t, n, r, i, a, !0));
}
function xi(e, t, n, r, i) {
	return yi(Ei(e, t, n, r, i, !0));
}
function Si(e) {
	return e ? e.__v_isVNode === !0 : !1;
}
function Ci(e, t) {
	return e.type === t.type && e.key === t.key;
}
var wi = ({ key: e }) => e ?? null, Ti = ({ ref: e, ref_key: t, ref_for: n }) => (typeof e == "number" && (e = "" + e), e == null ? null : g(e) || /* @__PURE__ */ B(e) || h(e) ? {
	i: G,
	r: e,
	k: t,
	f: !!n
} : e);
function X(e, t = null, n = null, r = 0, i = null, a = e === di ? 0 : 1, o = !1, s = !1) {
	let c = {
		__v_isVNode: !0,
		__v_skip: !0,
		type: e,
		props: t,
		key: t && wi(t),
		ref: t && Ti(t),
		scopeId: pn,
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
		ctx: G
	};
	return s ? (Ni(c, n), a & 128 && e.normalize(c)) : n && (c.shapeFlag |= g(n) ? 8 : 16), _i > 0 && !o && J && (c.patchFlag > 0 || a & 6) && c.patchFlag !== 32 && J.push(c), c;
}
var Ei = Di;
function Di(e, t = null, n = null, r = 0, i = null, a = !1) {
	if ((!e || e === $n) && (e = pi), Si(e)) {
		let r = ki(e, t, !0);
		return n && Ni(r, n), _i > 0 && !a && J && (r.shapeFlag & 6 ? J[J.indexOf(e)] = r : J.push(r)), r.patchFlag = -2, r;
	}
	if (ea(e) && (e = e.__vccOpts), t) {
		t = Oi(t);
		let { class: e, style: n } = t;
		e && !g(e) && (t.class = k(e)), v(n) && (/* @__PURE__ */ Nt(n) && !d(n) && (n = s({}, n)), t.style = ue(n));
	}
	let o = g(e) ? 1 : li(e) ? 128 : Dn(e) ? 64 : v(e) ? 4 : h(e) ? 2 : 0;
	return X(e, t, n, r, i, o, a, !0);
}
function Oi(e) {
	return e ? /* @__PURE__ */ Nt(e) || Lr(e) ? s({}, e) : e : null;
}
function ki(e, t, n = !1, r = !1) {
	let { props: i, ref: a, patchFlag: o, children: s, transition: c } = e, l = t ? Pi(i || {}, t) : i, u = {
		__v_isVNode: !0,
		__v_skip: !0,
		type: e.type,
		props: l,
		key: l && wi(l),
		ref: t && t.ref ? n && a ? d(a) ? a.concat(Ti(t)) : [a, Ti(t)] : Ti(t) : a,
		scopeId: e.scopeId,
		slotScopeIds: e.slotScopeIds,
		children: s,
		target: e.target,
		targetStart: e.targetStart,
		targetAnchor: e.targetAnchor,
		staticCount: e.staticCount,
		shapeFlag: e.shapeFlag,
		patchFlag: t && e.type !== di ? o === -1 ? 16 : o | 16 : o,
		dynamicProps: e.dynamicProps,
		dynamicChildren: e.dynamicChildren,
		appContext: e.appContext,
		dirs: e.dirs,
		transition: c,
		component: e.component,
		suspense: e.suspense,
		ssContent: e.ssContent && ki(e.ssContent),
		ssFallback: e.ssFallback && ki(e.ssFallback),
		placeholder: e.placeholder,
		el: e.el,
		anchor: e.anchor,
		ctx: e.ctx,
		ce: e.ce
	};
	return c && r && kn(u, c.clone(u)), u;
}
function Ai(e = " ", t = 0) {
	return Ei(fi, null, e, t);
}
function ji(e = "", t = !1) {
	return t ? (Y(), xi(pi, null, e)) : Ei(pi, null, e);
}
function Z(e) {
	return e == null || typeof e == "boolean" ? Ei(pi) : d(e) ? Ei(di, null, e.slice()) : Si(e) ? Mi(e) : Ei(fi, null, String(e));
}
function Mi(e) {
	return e.el === null && e.patchFlag !== -1 || e.memo ? e : ki(e);
}
function Ni(e, t) {
	let n = 0, { shapeFlag: r } = e;
	if (t == null) t = null;
	else if (d(t)) n = 16;
	else if (typeof t == "object") if (r & 65) {
		let n = t.default;
		n && (n._c && (n._d = !1), Ni(e, n()), n._c && (n._d = !0));
		return;
	} else {
		n = 32;
		let r = t._;
		!r && !Lr(t) ? t._ctx = G : r === 3 && G && (G.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
	}
	else h(t) ? (t = {
		default: t,
		_ctx: G
	}, n = 32) : (t = String(t), r & 64 ? (n = 16, t = [Ai(t)]) : n = 8);
	e.children = t, e.shapeFlag |= n;
}
function Pi(...e) {
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
function Q(e, t, n, r = null) {
	H(e, t, 7, [n, r]);
}
var Fi = yr(), Ii = 0;
function Li(e, n, r) {
	let i = e.type, a = (n ? n.appContext : e.appContext) || Fi, o = {
		uid: Ii++,
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
		scope: new xe(!0),
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
		propsOptions: Ur(i, a),
		emitsOptions: Er(i, a),
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
	return o.ctx = { _: o }, o.root = n ? n.root : o, o.emit = wr.bind(null, o), e.ce && e.ce(o), o;
}
var $ = null, Ri = () => $ || G, zi, Bi;
{
	let e = le(), t = (t, n) => {
		let r;
		return (r = e[t]) || (r = e[t] = []), r.push(n), (e) => {
			r.length > 1 ? r.forEach((t) => t(e)) : r[0](e);
		};
	};
	zi = t("__VUE_INSTANCE_SETTERS__", (e) => $ = e), Bi = t("__VUE_SSR_SETTERS__", (e) => Wi = e);
}
var Vi = (e) => {
	let t = $;
	return zi(e), e.scope.on(), () => {
		e.scope.off(), zi(t);
	};
}, Hi = () => {
	$ && $.scope.off(), zi(null);
};
function Ui(e) {
	return e.vnode.shapeFlag & 4;
}
var Wi = !1;
function Gi(e, t = !1, n = !1) {
	t && Bi(t);
	let { props: r, children: i } = e.vnode, a = Ui(e);
	Rr(e, r, a, t), Zr(e, i, n || t);
	let o = a ? Ki(e, t) : void 0;
	return t && Bi(!1), o;
}
function Ki(e, t) {
	let n = e.type;
	e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, rr);
	let { setup: r } = n;
	if (r) {
		Re();
		let n = e.setupContext = r.length > 1 ? Qi(e) : null, i = Vi(e), a = Yt(r, e, 0, [e.props, n]), o = y(a);
		if (ze(), i(), (o || e.sp) && !Fn(e) && An(e), o) {
			if (a.then(Hi, Hi), t) return a.then((n) => {
				qi(e, n, t);
			}).catch((t) => {
				Xt(t, e, 0);
			});
			e.asyncDep = a;
		} else qi(e, a, t);
	} else Xi(e, t);
}
function qi(e, t, n) {
	h(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : v(t) && (e.setupState = Vt(t)), Xi(e, n);
}
var Ji, Yi;
function Xi(e, t, n) {
	let i = e.type;
	if (!e.render) {
		if (!t && Ji && !i.render) {
			let t = i.template || ur(e).template;
			if (t) {
				let { isCustomElement: n, compilerOptions: r } = e.appContext.config, { delimiters: a, compilerOptions: o } = i;
				i.render = Ji(t, s(s({
					isCustomElement: n,
					delimiters: a
				}, r), o));
			}
		}
		e.render = i.render || r, Yi && Yi(e);
	}
	{
		let t = Vi(e);
		Re();
		try {
			or(e);
		} finally {
			ze(), t();
		}
	}
}
var Zi = { get(e, t) {
	return N(e, "get", ""), e[t];
} };
function Qi(e) {
	return {
		attrs: new Proxy(e.attrs, Zi),
		slots: e.slots,
		emit: e.emit,
		expose: (t) => {
			e.exposed = t || {};
		}
	};
}
function $i(e) {
	return e.exposed ? e.exposeProxy ||= new Proxy(Vt(Pt(e.exposed)), {
		get(t, n) {
			if (n in t) return t[n];
			if (n in tr) return tr[n](e);
		},
		has(e, t) {
			return t in e || t in tr;
		}
	}) : e.proxy;
}
function ea(e) {
	return h(e) && "__vccOpts" in e;
}
var ta = (e, t) => /* @__PURE__ */ Ut(e, t, Wi), na = "3.5.34", ra = void 0, ia = typeof window < "u" && window.trustedTypes;
if (ia) try {
	ra = /* @__PURE__ */ ia.createPolicy("vue", { createHTML: (e) => e });
} catch {}
var aa = ra ? (e) => ra.createHTML(e) : (e) => e, oa = "http://www.w3.org/2000/svg", sa = "http://www.w3.org/1998/Math/MathML", ca = typeof document < "u" ? document : null, la = ca && /* @__PURE__ */ ca.createElement("template"), ua = {
	insert: (e, t, n) => {
		t.insertBefore(e, n || null);
	},
	remove: (e) => {
		let t = e.parentNode;
		t && t.removeChild(e);
	},
	createElement: (e, t, n, r) => {
		let i = t === "svg" ? ca.createElementNS(oa, e) : t === "mathml" ? ca.createElementNS(sa, e) : n ? ca.createElement(e, { is: n }) : ca.createElement(e);
		return e === "select" && r && r.multiple != null && i.setAttribute("multiple", r.multiple), i;
	},
	createText: (e) => ca.createTextNode(e),
	createComment: (e) => ca.createComment(e),
	setText: (e, t) => {
		e.nodeValue = t;
	},
	setElementText: (e, t) => {
		e.textContent = t;
	},
	parentNode: (e) => e.parentNode,
	nextSibling: (e) => e.nextSibling,
	querySelector: (e) => ca.querySelector(e),
	setScopeId(e, t) {
		e.setAttribute(t, "");
	},
	insertStaticContent(e, t, n, r, i, a) {
		let o = n ? n.previousSibling : t.lastChild;
		if (i && (i === a || i.nextSibling)) for (; t.insertBefore(i.cloneNode(!0), n), !(i === a || !(i = i.nextSibling)););
		else {
			la.innerHTML = aa(r === "svg" ? `<svg>${e}</svg>` : r === "mathml" ? `<math>${e}</math>` : e);
			let i = la.content;
			if (r === "svg" || r === "mathml") {
				let e = i.firstChild;
				for (; e.firstChild;) i.appendChild(e.firstChild);
				i.removeChild(e);
			}
			t.insertBefore(i, n);
		}
		return [o ? o.nextSibling : t.firstChild, n ? n.previousSibling : t.lastChild];
	}
}, da = /* @__PURE__ */ Symbol("_vtc");
function fa(e, t, n) {
	let r = e[da];
	r && (t = (t ? [t, ...r] : [...r]).join(" ")), t == null ? e.removeAttribute("class") : n ? e.setAttribute("class", t) : e.className = t;
}
var pa = /* @__PURE__ */ Symbol("_vod"), ma = /* @__PURE__ */ Symbol("_vsh"), ha = /* @__PURE__ */ Symbol(""), ga = /(?:^|;)\s*display\s*:/;
function _a(e, t, n) {
	let r = e.style, i = g(n), a = !1;
	if (n && !i) {
		if (t) if (g(t)) for (let e of t.split(";")) {
			let t = e.slice(0, e.indexOf(":")).trim();
			n[t] ?? ya(r, t, "");
		}
		else for (let e in t) n[e] ?? ya(r, e, "");
		for (let i in n) {
			i === "display" && (a = !0);
			let o = n[i];
			o == null ? ya(r, i, "") : Ca(e, i, !g(t) && t ? t[i] : void 0, o) || ya(r, i, o);
		}
	} else if (i) {
		if (t !== n) {
			let e = r[ha];
			e && (n += ";" + e), r.cssText = n, a = ga.test(n);
		}
	} else t && e.removeAttribute("style");
	pa in e && (e[pa] = a ? r.display : "", e[ma] && (r.display = "none"));
}
var va = /\s*!important$/;
function ya(e, t, n) {
	if (d(n)) n.forEach((n) => ya(e, t, n));
	else if (n ??= "", t.startsWith("--")) e.setProperty(t, n);
	else {
		let r = Sa(e, t);
		va.test(n) ? e.setProperty(E(r), n.replace(va, ""), "important") : e[r] = n;
	}
}
var ba = [
	"Webkit",
	"Moz",
	"ms"
], xa = {};
function Sa(e, t) {
	let n = xa[t];
	if (n) return n;
	let r = T(t);
	if (r !== "filter" && r in e) return xa[t] = r;
	r = ie(r);
	for (let n = 0; n < ba.length; n++) {
		let i = ba[n] + r;
		if (i in e) return xa[t] = i;
	}
	return t;
}
function Ca(e, t, n, r) {
	return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && g(r) && n === r;
}
var wa = "http://www.w3.org/1999/xlink";
function Ta(e, t, n, r, i, a = ge(t)) {
	r && t.startsWith("xlink:") ? n == null ? e.removeAttributeNS(wa, t.slice(6, t.length)) : e.setAttributeNS(wa, t, n) : n == null || a && !_e(n) ? e.removeAttribute(t) : e.setAttribute(t, a ? "" : _(n) ? String(n) : n);
}
function Ea(e, t, n, r, i) {
	if (t === "innerHTML" || t === "textContent") {
		n != null && (e[t] = t === "innerHTML" ? aa(n) : n);
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
function Da(e, t, n, r) {
	e.addEventListener(t, n, r);
}
function Oa(e, t, n, r) {
	e.removeEventListener(t, n, r);
}
var ka = /* @__PURE__ */ Symbol("_vei");
function Aa(e, t, n, r, i = null) {
	let a = e[ka] || (e[ka] = {}), o = a[t];
	if (r && o) o.value = r;
	else {
		let [n, s] = Ma(t);
		r ? Da(e, n, a[t] = Ia(r, i), s) : o && (Oa(e, n, o, s), a[t] = void 0);
	}
}
var ja = /(?:Once|Passive|Capture)$/;
function Ma(e) {
	let t;
	if (ja.test(e)) {
		t = {};
		let n;
		for (; n = e.match(ja);) e = e.slice(0, e.length - n[0].length), t[n[0].toLowerCase()] = !0;
	}
	return [e[2] === ":" ? e.slice(3) : E(e.slice(2)), t];
}
var Na = 0, Pa = /* @__PURE__ */ Promise.resolve(), Fa = () => Na ||= (Pa.then(() => Na = 0), Date.now());
function Ia(e, t) {
	let n = (e) => {
		if (!e._vts) e._vts = Date.now();
		else if (e._vts <= n.attached) return;
		H(La(e, n.value), t, 5, [e]);
	};
	return n.value = e, n.attached = Fa(), n;
}
function La(e, t) {
	if (d(t)) {
		let n = e.stopImmediatePropagation;
		return e.stopImmediatePropagation = () => {
			n.call(e), e._stopped = !0;
		}, t.map((e) => (t) => !t._stopped && e && e(t));
	} else return t;
}
var Ra = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, za = (e, t, n, r, i, s) => {
	let c = i === "svg";
	t === "class" ? fa(e, r, c) : t === "style" ? _a(e, n, r) : a(t) ? o(t) || Aa(e, t, n, r, s) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : Ba(e, t, r, c)) ? (Ea(e, t, r), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && Ta(e, t, r, c, s, t !== "value")) : e._isVueCE && (Va(e, t) || e._def.__asyncLoader && (/[A-Z]/.test(t) || !g(r))) ? Ea(e, T(t), r, s, t) : (t === "true-value" ? e._trueValue = r : t === "false-value" && (e._falseValue = r), Ta(e, t, r, c));
};
function Ba(e, t, n, r) {
	if (r) return !!(t === "innerHTML" || t === "textContent" || t in e && Ra(t) && h(n));
	if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA") return !1;
	if (t === "width" || t === "height") {
		let t = e.tagName;
		if (t === "IMG" || t === "VIDEO" || t === "CANVAS" || t === "SOURCE") return !1;
	}
	return Ra(t) && g(n) ? !1 : t in e;
}
function Va(e, t) {
	let n = e._def.props;
	if (!n) return !1;
	let r = T(t);
	return Array.isArray(n) ? n.some((e) => T(e) === r) : Object.keys(n).some((e) => T(e) === r);
}
var Ha = (e) => {
	let t = e.props["onUpdate:modelValue"] || !1;
	return d(t) ? (e) => oe(t, e) : t;
}, Ua = /* @__PURE__ */ Symbol("_assign"), Wa = {
	deep: !0,
	created(e, { value: t, modifiers: { number: n } }, r) {
		let i = p(t);
		Da(e, "change", () => {
			let t = Array.prototype.filter.call(e.options, (e) => e.selected).map((e) => n ? se(Ka(e)) : Ka(e));
			e[Ua](e.multiple ? i ? new Set(t) : t : t[0]), e._assigning = !0, rn(() => {
				e._assigning = !1;
			});
		}), e[Ua] = Ha(r);
	},
	mounted(e, { value: t }) {
		Ga(e, t);
	},
	beforeUpdate(e, t, n) {
		e[Ua] = Ha(n);
	},
	updated(e, { value: t }) {
		e._assigning || Ga(e, t);
	}
};
function Ga(e, t) {
	let n = e.multiple, r = d(t);
	if (!(n && !r && !p(t))) {
		for (let i = 0, a = e.options.length; i < a; i++) {
			let a = e.options[i], o = Ka(a);
			if (n) if (r) {
				let e = typeof o;
				e === "string" || e === "number" ? a.selected = t.some((e) => String(e) === String(o)) : a.selected = be(t, o) > -1;
			} else a.selected = t.has(o);
			else if (ye(Ka(a), t)) {
				e.selectedIndex !== i && (e.selectedIndex = i);
				return;
			}
		}
		!n && e.selectedIndex !== -1 && (e.selectedIndex = -1);
	}
}
function Ka(e) {
	return "_value" in e ? e._value : e.value;
}
var qa = [
	"ctrl",
	"shift",
	"alt",
	"meta"
], Ja = {
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
	exact: (e, t) => qa.some((n) => e[`${n}Key`] && !t.includes(n))
}, Ya = (e, t) => {
	if (!e) return e;
	let n = e._withMods ||= {}, r = t.join(".");
	return n[r] || (n[r] = ((n, ...r) => {
		for (let e = 0; e < t.length; e++) {
			let r = Ja[t[e]];
			if (r && r(n, t)) return;
		}
		return e(n, ...r);
	}));
}, Xa = /* @__PURE__ */ s({ patchProp: za }, ua), Za;
function Qa() {
	return Za ||= $r(Xa);
}
var $a = ((...e) => {
	let t = Qa().createApp(...e), { mount: n } = t;
	return t.mount = (e) => {
		let r = to(e);
		if (!r) return;
		let i = t._component;
		!h(i) && !i.render && !i.template && (i.template = r.innerHTML), r.nodeType === 1 && (r.textContent = "");
		let a = n(r, !1, eo(r));
		return r instanceof Element && (r.removeAttribute("v-cloak"), r.setAttribute("data-v-app", "")), a;
	}, t;
});
function eo(e) {
	if (e instanceof SVGElement) return "svg";
	if (typeof MathMLElement == "function" && e instanceof MathMLElement) return "mathml";
}
function to(e) {
	return g(e) ? document.querySelector(e) : e;
}
//#endregion
//#region \0plugin-vue:export-helper
var no = (e, t) => {
	let n = e.__vccOpts || e;
	for (let [e, r] of t) n[e] = r;
	return n;
}, ro = {}, io = { class: "card" };
function ao(e, t) {
	return Y(), bi("div", io, [...t[0] ||= [X("h2", null, "Suche", -1), X("p", null, "Such-Funktionalität kommt hierhin.", -1)]]);
}
var oo = /* @__PURE__ */ no(ro, [["render", ao]]), so = { class: "card" }, co = { class: "field" }, lo = ["disabled"], uo = "a31b415a-5264-419d-b152-8c8192a54994", fo = {
	__name: "SettingsTab",
	setup(e) {
		let t = window.ApiClient ?? null, n = /* @__PURE__ */ It("VueJS"), r = /* @__PURE__ */ It(!1);
		Wn(async () => {
			t && (n.value = (await t.getPluginConfiguration(uo)).ActiveWebUi ?? "VueJS");
		});
		async function i() {
			r.value = !0;
			try {
				let e = await t.getPluginConfiguration(uo);
				e.ActiveWebUi = n.value, await t.updatePluginConfiguration(uo, e);
			} finally {
				r.value = !1;
			}
		}
		return (e, t) => (Y(), bi("div", so, [t[3] ||= X("h2", null, "Einstellungen", -1), X("form", { onSubmit: Ya(i, ["prevent"]) }, [X("div", co, [t[2] ||= X("label", {
			class: "field-label",
			for: "optActiveWebUi"
		}, "Aktive WebUI", -1), gn(X("select", {
			id: "optActiveWebUi",
			"onUpdate:modelValue": t[0] ||= (e) => n.value = e,
			class: "field-select"
		}, [...t[1] ||= [
			X("option", { value: "VueJS" }, "VueJS", -1),
			X("option", { value: "ShowBoth" }, "Beide anzeigen", -1),
			X("option", { value: "Html" }, "HTML Legacy", -1)
		]], 512), [[Wa, n.value]])]), X("button", {
			type: "submit",
			class: "btn btn-save",
			disabled: r.value
		}, "Speichern", 8, lo)], 32)]));
	}
}, po = {}, mo = { class: "card" };
function ho(e, t) {
	return Y(), bi("div", mo, [...t[0] ||= [X("h2", null, "Abo Verwaltung", -1), X("p", null, "Abo-Liste kommt hierhin.", -1)]]);
}
var go = /* @__PURE__ */ no(po, [["render", ho]]), _o = {}, vo = { class: "card" };
function yo(e, t) {
	return Y(), bi("div", vo, [...t[0] ||= [X("h2", null, "Downloads", -1), X("p", null, "Download-Verlauf kommt hierhin.", -1)]]);
}
var bo = /* @__PURE__ */ no(_o, [["render", yo]]), xo = { class: "plugin-config" }, So = { class: "tab-row" }, Co = { class: "tab-content" }, wo = /* @__PURE__ */ no({
	__name: "PluginConfig",
	setup(e) {
		let t = /* @__PURE__ */ It("search");
		return (e, n) => (Y(), bi("div", xo, [
			n[4] ||= X("header", { class: "config-header" }, [X("h1", { class: "config-title" }, "MediathekViewDL"), X("p", { class: "config-subtitle" }, "Plugin Konfiguration")], -1),
			X("div", So, [
				X("button", {
					class: k(["tab-btn", { active: t.value === "search" }]),
					onClick: n[0] ||= (e) => t.value = "search"
				}, "Suche", 2),
				X("button", {
					class: k(["tab-btn", { active: t.value === "settings" }]),
					onClick: n[1] ||= (e) => t.value = "settings"
				}, "Einstellungen", 2),
				X("button", {
					class: k(["tab-btn", { active: t.value === "subscriptions" }]),
					onClick: n[2] ||= (e) => t.value = "subscriptions"
				}, "Abos", 2),
				X("button", {
					class: k(["tab-btn", { active: t.value === "downloads" }]),
					onClick: n[3] ||= (e) => t.value = "downloads"
				}, "Downloads", 2)
			]),
			X("div", Co, [
				t.value === "search" ? (Y(), xi(oo, { key: 0 })) : ji("", !0),
				t.value === "settings" ? (Y(), xi(fo, { key: 1 })) : ji("", !0),
				t.value === "subscriptions" ? (Y(), xi(go, { key: 2 })) : ji("", !0),
				t.value === "downloads" ? (Y(), xi(bo, { key: 3 })) : ji("", !0)
			])
		]));
	}
}, [["__scopeId", "data-v-a62a1791"]]), To = {
	__name: "App",
	setup(e) {
		return (e, t) => (Y(), xi(wo));
	}
}, Eo = null;
function Do(e) {
	Eo || (Eo = $a(To), Eo.mount(e));
}
function Oo() {
	Eo &&= (Eo.unmount(), null);
}
var ko = document.getElementById("configPageVueJSPage"), Ao = !window.Dashboard;
if (ko) if (Ao) {
	let e = ko.querySelector("#app");
	e && Do(e);
} else ko.addEventListener("pageshow", (e) => {
	let t = ko.querySelector("#app");
	t && (Oo(), Do(t));
}), ko.addEventListener("pagehide", () => {
	Oo();
});
//#endregion
