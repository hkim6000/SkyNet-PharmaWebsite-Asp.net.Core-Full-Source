var InvestorsJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('iv-nav').classList.add('iv-open');
        el('iv-scrim').classList.add('iv-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('iv-nav').classList.remove('iv-open');
        el('iv-scrim').classList.remove('iv-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.iv-mi');
        if (li) {
            li.classList.toggle('iv-exp');
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
            if (q === lastQ && el('iv-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('Investors/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('iv-sugg').classList.add('iv-open');
    }

    function closeSugg() {
        el('iv-sugg').classList.remove('iv-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.iv-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('Investors/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.iv-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('iv-act');
        }
        btn.classList.add('iv-act');
        el('iv-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.iv-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#iv-grid .iv-ncard').length) });
        $WaitOn();
        $ApiRequest('Investors/More', JSON.stringify(list));
    }

    function chart(btn, range) {
        var btns = btn.parentNode.querySelectorAll('.iv-rng');
        for (var i = 0; i < btns.length; i++) {
            btns[i].classList.toggle('iv-act', btns[i] === btn);
        }
        $ApiRequest('Investors/Chart', JSON.stringify([{ key: 'range', vlu: range }]));
    }

    function detail(btn, id) {
        var box = el('iv-d-' + id);
        if (!box) {
            return;
        }
        if (box.innerHTML !== '') {
            var open = box.classList.toggle('iv-open');
            btn.classList.toggle('iv-act', open);
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
            return;
        }
        btn.classList.add('iv-act');
        btn.setAttribute('aria-expanded', 'true');
        $ApiRequest('Investors/Detail', JSON.stringify([{ key: 'id', vlu: id }]));
    }

    function opened(id) {
        var box = el('iv-d-' + id);
        if (box) {
            box.classList.add('iv-open');
        }
    }

    function send() {
        $WaitOn();
        $ApiRequest('Investors/Send', JSON.stringify([
            { key: 'name', vlu: el('iv-name').value },
            { key: 'email', vlu: el('iv-email').value },
            { key: 'topic', vlu: el('iv-topic').value },
            { key: 'message', vlu: el('iv-msg').value }
        ]));
    }

    function sent() {
        el('iv-name').value = '';
        el('iv-email').value = '';
        el('iv-topic').value = '';
        el('iv-msg').value = '';
    }

    function preset() {
        var fields = document.querySelectorAll('select.iv-fv');
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
            var box = el('iv-search');
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
            var h = el('iv-head');
            if (h) {
                h.classList.toggle('iv-scrolled', window.pageYOffset > 8);
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

InvestorsJs.reveal();
