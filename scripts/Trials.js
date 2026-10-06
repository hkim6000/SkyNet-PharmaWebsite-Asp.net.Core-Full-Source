var TrialsJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('ct-nav').classList.add('ct-open');
        el('ct-scrim').classList.add('ct-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('ct-nav').classList.remove('ct-open');
        el('ct-scrim').classList.remove('ct-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.ct-mi');
        if (li) {
            li.classList.toggle('ct-exp');
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
            if (q === lastQ && el('ct-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('Trials/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('ct-sugg').classList.add('ct-open');
    }

    function closeSugg() {
        el('ct-sugg').classList.remove('ct-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.ct-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('Trials/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.ct-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('ct-act');
        }
        btn.classList.add('ct-act');
        el('ct-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.ct-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#ct-grid .ct-ncard').length) });
        $WaitOn();
        $ApiRequest('Trials/More', JSON.stringify(list));
    }

    function chart(btn, range) {
        var btns = btn.parentNode.querySelectorAll('.ct-rng');
        for (var i = 0; i < btns.length; i++) {
            btns[i].classList.toggle('ct-act', btns[i] === btn);
        }
        $ApiRequest('Trials/Chart', JSON.stringify([{ key: 'range', vlu: range }]));
    }

    function detail(btn, id) {
        var box = el('ct-d-' + id);
        if (!box) {
            return;
        }
        if (box.innerHTML !== '') {
            var open = box.classList.toggle('ct-open');
            btn.classList.toggle('ct-act', open);
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
            return;
        }
        btn.classList.add('ct-act');
        btn.setAttribute('aria-expanded', 'true');
        $ApiRequest('Trials/Detail', JSON.stringify([{ key: 'id', vlu: id }]));
    }

    function opened(id) {
        var box = el('ct-d-' + id);
        if (box) {
            box.classList.add('ct-open');
        }
    }

    function send() {
        $WaitOn();
        $ApiRequest('Trials/Send', JSON.stringify([
            { key: 'name', vlu: el('ct-name').value },
            { key: 'email', vlu: el('ct-email').value },
            { key: 'topic', vlu: el('ct-topic').value },
            { key: 'message', vlu: el('ct-msg').value }
        ]));
    }

    function sent() {
        el('ct-name').value = '';
        el('ct-email').value = '';
        el('ct-topic').value = '';
        el('ct-msg').value = '';
    }

    function preset() {
        var fields = document.querySelectorAll('select.ct-fv');
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
            var box = el('ct-search');
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
            var h = el('ct-head');
            if (h) {
                h.classList.toggle('ct-scrolled', window.pageYOffset > 8);
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

TrialsJs.reveal();
