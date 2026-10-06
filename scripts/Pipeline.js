var PipelineJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('pl-nav').classList.add('pl-open');
        el('pl-scrim').classList.add('pl-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('pl-nav').classList.remove('pl-open');
        el('pl-scrim').classList.remove('pl-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.pl-mi');
        if (li) {
            li.classList.toggle('pl-exp');
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
            if (q === lastQ && el('pl-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('Pipeline/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('pl-sugg').classList.add('pl-open');
    }

    function closeSugg() {
        el('pl-sugg').classList.remove('pl-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.pl-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('Pipeline/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.pl-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('pl-act');
        }
        btn.classList.add('pl-act');
        el('pl-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.pl-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#pl-grid .pl-ncard').length) });
        $WaitOn();
        $ApiRequest('Pipeline/More', JSON.stringify(list));
    }

    function chart(btn, range) {
        var btns = btn.parentNode.querySelectorAll('.pl-rng');
        for (var i = 0; i < btns.length; i++) {
            btns[i].classList.toggle('pl-act', btns[i] === btn);
        }
        $ApiRequest('Pipeline/Chart', JSON.stringify([{ key: 'range', vlu: range }]));
    }

    function detail(btn, id) {
        var box = el('pl-d-' + id);
        if (!box) {
            return;
        }
        if (box.innerHTML !== '') {
            var open = box.classList.toggle('pl-open');
            btn.classList.toggle('pl-act', open);
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
            return;
        }
        btn.classList.add('pl-act');
        btn.setAttribute('aria-expanded', 'true');
        $ApiRequest('Pipeline/Detail', JSON.stringify([{ key: 'id', vlu: id }]));
    }

    function opened(id) {
        var box = el('pl-d-' + id);
        if (box) {
            box.classList.add('pl-open');
        }
    }

    function send() {
        $WaitOn();
        $ApiRequest('Pipeline/Send', JSON.stringify([
            { key: 'name', vlu: el('pl-name').value },
            { key: 'email', vlu: el('pl-email').value },
            { key: 'topic', vlu: el('pl-topic').value },
            { key: 'message', vlu: el('pl-msg').value }
        ]));
    }

    function sent() {
        el('pl-name').value = '';
        el('pl-email').value = '';
        el('pl-topic').value = '';
        el('pl-msg').value = '';
    }

    function preset() {
        var fields = document.querySelectorAll('select.pl-fv');
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
            var box = el('pl-search');
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
            var h = el('pl-head');
            if (h) {
                h.classList.toggle('pl-scrolled', window.pageYOffset > 8);
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

PipelineJs.reveal();
