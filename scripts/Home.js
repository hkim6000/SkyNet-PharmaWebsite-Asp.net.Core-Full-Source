var HomeJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('hm-nav').classList.add('hm-open');
        el('hm-scrim').classList.add('hm-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('hm-nav').classList.remove('hm-open');
        el('hm-scrim').classList.remove('hm-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.hm-mi');
        if (li) {
            li.classList.toggle('hm-exp');
        }
    }

    function search(value) {
        var q = (value || '').trim();
        clearTimeout(timer);
        if (q.length < 2) {
            closeSugg();
            lastQ = '';
            return;
        }
        timer = setTimeout(function () {
            if (q === lastQ && el('hm-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('Home/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('hm-sugg').classList.add('hm-open');
    }

    function closeSugg() {
        el('hm-sugg').classList.remove('hm-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.hm-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('Home/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.hm-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('hm-act');
        }
        btn.classList.add('hm-act');
        el('hm-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.hm-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#hm-grid .hm-ncard').length) });
        $WaitOn();
        $ApiRequest('Home/More', JSON.stringify(list));
    }

    function chart(btn, range) {
        var btns = btn.parentNode.querySelectorAll('.hm-rng');
        for (var i = 0; i < btns.length; i++) {
            btns[i].classList.toggle('hm-act', btns[i] === btn);
        }
        $ApiRequest('Home/Chart', JSON.stringify([{ key: 'range', vlu: range }]));
    }

    function detail(btn, id) {
        var box = el('hm-d-' + id);
        if (!box) {
            return;
        }
        if (box.innerHTML !== '') {
            var open = box.classList.toggle('hm-open');
            btn.classList.toggle('hm-act', open);
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
            return;
        }
        btn.classList.add('hm-act');
        btn.setAttribute('aria-expanded', 'true');
        $ApiRequest('Home/Detail', JSON.stringify([{ key: 'id', vlu: id }]));
    }

    function opened(id) {
        var box = el('hm-d-' + id);
        if (box) {
            box.classList.add('hm-open');
        }
    }

    function send() {
        $WaitOn();
        $ApiRequest('Home/Send', JSON.stringify([
            { key: 'name', vlu: el('hm-name').value },
            { key: 'email', vlu: el('hm-email').value },
            { key: 'topic', vlu: el('hm-topic').value },
            { key: 'message', vlu: el('hm-msg').value }
        ]));
    }

    function sent() {
        el('hm-name').value = '';
        el('hm-email').value = '';
        el('hm-topic').value = '';
        el('hm-msg').value = '';
    }

    function preset() {
        var fields = document.querySelectorAll('select.hm-fv');
        for (var i = 0; i < fields.length; i++) {
            var v = fields[i].getAttribute('data-v');
            if (v) {
                fields[i].value = v;
            }
        }
    }

    function reveal() {
        preset();
        document.addEventListener('click', function (e) {
            var box = el('hm-search');
            if (box && !box.contains(e.target)) {
                closeSugg();
            }
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closeSugg();
                closeNav();
            }
        });
        window.addEventListener('scroll', function () {
            var h = el('hm-head');
            if (h) {
                h.classList.toggle('hm-scrolled', window.pageYOffset > 8);
            }
        }, { passive: true });
        window.addEventListener('resize', function () {
            if (window.innerWidth > 767) {
                closeNav();
            }
        });
    }

    return {
        reveal: function () { reveal(); },
        openNav: function () { openNav(); },
        closeNav: function () { closeNav(); },
        toggleSub: function (btn) { toggleSub(btn); },
        search: function (v) { search(v); },
        openSugg: function () { openSugg(); },
        closeSugg: function () { closeSugg(); },
        filter: function () { filter(); },
        chip: function (btn, g, v) { chip(btn, g, v); },
        typed: function () { typed(); },
        reset: function () { reset(); },
        more: function () { more(); },
        chart: function (btn, r) { chart(btn, r); },
        detail: function (btn, id) { detail(btn, id); },
        opened: function (id) { opened(id); },
        send: function () { send(); },
        sent: function () { sent(); }
    };

})();

HomeJs.reveal();
