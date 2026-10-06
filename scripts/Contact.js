var ContactJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('co-nav').classList.add('co-open');
        el('co-scrim').classList.add('co-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('co-nav').classList.remove('co-open');
        el('co-scrim').classList.remove('co-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.co-mi');
        if (li) {
            li.classList.toggle('co-exp');
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
            if (q === lastQ && el('co-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('Contact/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('co-sugg').classList.add('co-open');
    }

    function closeSugg() {
        el('co-sugg').classList.remove('co-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.co-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('Contact/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.co-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('co-act');
        }
        btn.classList.add('co-act');
        el('co-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.co-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#co-grid .co-ncard').length) });
        $WaitOn();
        $ApiRequest('Contact/More', JSON.stringify(list));
    }

    function chart(btn, range) {
        var btns = btn.parentNode.querySelectorAll('.co-rng');
        for (var i = 0; i < btns.length; i++) {
            btns[i].classList.toggle('co-act', btns[i] === btn);
        }
        $ApiRequest('Contact/Chart', JSON.stringify([{ key: 'range', vlu: range }]));
    }

    function detail(btn, id) {
        var box = el('co-d-' + id);
        if (!box) {
            return;
        }
        if (box.innerHTML !== '') {
            var open = box.classList.toggle('co-open');
            btn.classList.toggle('co-act', open);
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
            return;
        }
        btn.classList.add('co-act');
        btn.setAttribute('aria-expanded', 'true');
        $ApiRequest('Contact/Detail', JSON.stringify([{ key: 'id', vlu: id }]));
    }

    function opened(id) {
        var box = el('co-d-' + id);
        if (box) {
            box.classList.add('co-open');
        }
    }

    function send() {
        $WaitOn();
        $ApiRequest('Contact/Send', JSON.stringify([
            { key: 'name', vlu: el('co-name').value },
            { key: 'email', vlu: el('co-email').value },
            { key: 'topic', vlu: el('co-topic').value },
            { key: 'message', vlu: el('co-msg').value }
        ]));
    }

    function sent() {
        el('co-name').value = '';
        el('co-email').value = '';
        el('co-topic').value = '';
        el('co-msg').value = '';
    }

    function preset() {
        var fields = document.querySelectorAll('select.co-fv');
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
            var box = el('co-search');
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
            var h = el('co-head');
            if (h) {
                h.classList.toggle('co-scrolled', window.pageYOffset > 8);
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

ContactJs.reveal();
