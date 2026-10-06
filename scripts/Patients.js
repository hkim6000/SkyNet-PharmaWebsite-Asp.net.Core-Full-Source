var PatientsJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('pt-nav').classList.add('pt-open');
        el('pt-scrim').classList.add('pt-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('pt-nav').classList.remove('pt-open');
        el('pt-scrim').classList.remove('pt-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.pt-mi');
        if (li) {
            li.classList.toggle('pt-exp');
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
            if (q === lastQ && el('pt-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('Patients/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('pt-sugg').classList.add('pt-open');
    }

    function closeSugg() {
        el('pt-sugg').classList.remove('pt-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.pt-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('Patients/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.pt-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('pt-act');
        }
        btn.classList.add('pt-act');
        el('pt-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.pt-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#pt-grid .pt-ncard').length) });
        $WaitOn();
        $ApiRequest('Patients/More', JSON.stringify(list));
    }

    function chart(btn, range) {
        var btns = btn.parentNode.querySelectorAll('.pt-rng');
        for (var i = 0; i < btns.length; i++) {
            btns[i].classList.toggle('pt-act', btns[i] === btn);
        }
        $ApiRequest('Patients/Chart', JSON.stringify([{ key: 'range', vlu: range }]));
    }

    function detail(btn, id) {
        var box = el('pt-d-' + id);
        if (!box) {
            return;
        }
        if (box.innerHTML !== '') {
            var open = box.classList.toggle('pt-open');
            btn.classList.toggle('pt-act', open);
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
            return;
        }
        btn.classList.add('pt-act');
        btn.setAttribute('aria-expanded', 'true');
        $ApiRequest('Patients/Detail', JSON.stringify([{ key: 'id', vlu: id }]));
    }

    function opened(id) {
        var box = el('pt-d-' + id);
        if (box) {
            box.classList.add('pt-open');
        }
    }

    function send() {
        $WaitOn();
        $ApiRequest('Patients/Send', JSON.stringify([
            { key: 'name', vlu: el('pt-name').value },
            { key: 'email', vlu: el('pt-email').value },
            { key: 'topic', vlu: el('pt-topic').value },
            { key: 'message', vlu: el('pt-msg').value }
        ]));
    }

    function sent() {
        el('pt-name').value = '';
        el('pt-email').value = '';
        el('pt-topic').value = '';
        el('pt-msg').value = '';
    }

    function preset() {
        var fields = document.querySelectorAll('select.pt-fv');
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
            var box = el('pt-search');
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
            var h = el('pt-head');
            if (h) {
                h.classList.toggle('pt-scrolled', window.pageYOffset > 8);
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

PatientsJs.reveal();
