var AboutJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('ab-nav').classList.add('ab-open');
        el('ab-scrim').classList.add('ab-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('ab-nav').classList.remove('ab-open');
        el('ab-scrim').classList.remove('ab-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.ab-mi');
        if (li) {
            li.classList.toggle('ab-exp');
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
            if (q === lastQ && el('ab-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('About/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('ab-sugg').classList.add('ab-open');
    }

    function closeSugg() {
        el('ab-sugg').classList.remove('ab-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.ab-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('About/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.ab-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('ab-act');
        }
        btn.classList.add('ab-act');
        el('ab-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.ab-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#ab-grid .ab-ncard').length) });
        $WaitOn();
        $ApiRequest('About/More', JSON.stringify(list));
    }

    function chart(btn, range) {
        var btns = btn.parentNode.querySelectorAll('.ab-rng');
        for (var i = 0; i < btns.length; i++) {
            btns[i].classList.toggle('ab-act', btns[i] === btn);
        }
        $ApiRequest('About/Chart', JSON.stringify([{ key: 'range', vlu: range }]));
    }

    function detail(btn, id) {
        var box = el('ab-d-' + id);
        if (!box) {
            return;
        }
        if (box.innerHTML !== '') {
            var open = box.classList.toggle('ab-open');
            btn.classList.toggle('ab-act', open);
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
            return;
        }
        btn.classList.add('ab-act');
        btn.setAttribute('aria-expanded', 'true');
        $ApiRequest('About/Detail', JSON.stringify([{ key: 'id', vlu: id }]));
    }

    function opened(id) {
        var box = el('ab-d-' + id);
        if (box) {
            box.classList.add('ab-open');
        }
    }

    function send() {
        $WaitOn();
        $ApiRequest('About/Send', JSON.stringify([
            { key: 'name', vlu: el('ab-name').value },
            { key: 'email', vlu: el('ab-email').value },
            { key: 'topic', vlu: el('ab-topic').value },
            { key: 'message', vlu: el('ab-msg').value }
        ]));
    }

    function sent() {
        el('ab-name').value = '';
        el('ab-email').value = '';
        el('ab-topic').value = '';
        el('ab-msg').value = '';
    }

    function preset() {
        var fields = document.querySelectorAll('select.ab-fv');
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
            var box = el('ab-search');
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
            var h = el('ab-head');
            if (h) {
                h.classList.toggle('ab-scrolled', window.pageYOffset > 8);
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

AboutJs.reveal();
