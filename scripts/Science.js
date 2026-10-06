var ScienceJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('sc-nav').classList.add('sc-open');
        el('sc-scrim').classList.add('sc-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('sc-nav').classList.remove('sc-open');
        el('sc-scrim').classList.remove('sc-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.sc-mi');
        if (li) {
            li.classList.toggle('sc-exp');
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
            if (q === lastQ && el('sc-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('Science/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('sc-sugg').classList.add('sc-open');
    }

    function closeSugg() {
        el('sc-sugg').classList.remove('sc-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.sc-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('Science/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.sc-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('sc-act');
        }
        btn.classList.add('sc-act');
        el('sc-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.sc-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#sc-grid .sc-ncard').length) });
        $WaitOn();
        $ApiRequest('Science/More', JSON.stringify(list));
    }

    function chart(btn, range) {
        var btns = btn.parentNode.querySelectorAll('.sc-rng');
        for (var i = 0; i < btns.length; i++) {
            btns[i].classList.toggle('sc-act', btns[i] === btn);
        }
        $ApiRequest('Science/Chart', JSON.stringify([{ key: 'range', vlu: range }]));
    }

    function detail(btn, id) {
        var box = el('sc-d-' + id);
        if (!box) {
            return;
        }
        if (box.innerHTML !== '') {
            var open = box.classList.toggle('sc-open');
            btn.classList.toggle('sc-act', open);
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
            return;
        }
        btn.classList.add('sc-act');
        btn.setAttribute('aria-expanded', 'true');
        $ApiRequest('Science/Detail', JSON.stringify([{ key: 'id', vlu: id }]));
    }

    function opened(id) {
        var box = el('sc-d-' + id);
        if (box) {
            box.classList.add('sc-open');
        }
    }

    function send() {
        $WaitOn();
        $ApiRequest('Science/Send', JSON.stringify([
            { key: 'name', vlu: el('sc-name').value },
            { key: 'email', vlu: el('sc-email').value },
            { key: 'topic', vlu: el('sc-topic').value },
            { key: 'message', vlu: el('sc-msg').value }
        ]));
    }

    function sent() {
        el('sc-name').value = '';
        el('sc-email').value = '';
        el('sc-topic').value = '';
        el('sc-msg').value = '';
    }

    function preset() {
        var fields = document.querySelectorAll('select.sc-fv');
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
            var box = el('sc-search');
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
            var h = el('sc-head');
            if (h) {
                h.classList.toggle('sc-scrolled', window.pageYOffset > 8);
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

ScienceJs.reveal();
