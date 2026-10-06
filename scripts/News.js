var NewsJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('nw-nav').classList.add('nw-open');
        el('nw-scrim').classList.add('nw-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('nw-nav').classList.remove('nw-open');
        el('nw-scrim').classList.remove('nw-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.nw-mi');
        if (li) {
            li.classList.toggle('nw-exp');
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
            if (q === lastQ && el('nw-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('News/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('nw-sugg').classList.add('nw-open');
    }

    function closeSugg() {
        el('nw-sugg').classList.remove('nw-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.nw-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('News/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.nw-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('nw-act');
        }
        btn.classList.add('nw-act');
        el('nw-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.nw-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#nw-grid .nw-ncard').length) });
        $WaitOn();
        $ApiRequest('News/More', JSON.stringify(list));
    }

    function chart(btn, range) {
        var btns = btn.parentNode.querySelectorAll('.nw-rng');
        for (var i = 0; i < btns.length; i++) {
            btns[i].classList.toggle('nw-act', btns[i] === btn);
        }
        $ApiRequest('News/Chart', JSON.stringify([{ key: 'range', vlu: range }]));
    }

    function detail(btn, id) {
        var box = el('nw-d-' + id);
        if (!box) {
            return;
        }
        if (box.innerHTML !== '') {
            var open = box.classList.toggle('nw-open');
            btn.classList.toggle('nw-act', open);
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
            return;
        }
        btn.classList.add('nw-act');
        btn.setAttribute('aria-expanded', 'true');
        $ApiRequest('News/Detail', JSON.stringify([{ key: 'id', vlu: id }]));
    }

    function opened(id) {
        var box = el('nw-d-' + id);
        if (box) {
            box.classList.add('nw-open');
        }
    }

    function send() {
        $WaitOn();
        $ApiRequest('News/Send', JSON.stringify([
            { key: 'name', vlu: el('nw-name').value },
            { key: 'email', vlu: el('nw-email').value },
            { key: 'topic', vlu: el('nw-topic').value },
            { key: 'message', vlu: el('nw-msg').value }
        ]));
    }

    function sent() {
        el('nw-name').value = '';
        el('nw-email').value = '';
        el('nw-topic').value = '';
        el('nw-msg').value = '';
    }

    function preset() {
        var fields = document.querySelectorAll('select.nw-fv');
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
            var box = el('nw-search');
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
            var h = el('nw-head');
            if (h) {
                h.classList.toggle('nw-scrolled', window.pageYOffset > 8);
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

NewsJs.reveal();
