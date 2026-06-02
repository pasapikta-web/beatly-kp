document.addEventListener('DOMContentLoaded', function () {
    hideLoader();
});

window.addEventListener('load', function () {
    hideLoader();
});

window.addEventListener('pageshow', function () {
    hideLoader();
});

document.addEventListener('input', function (e) {
    if (e.target.matches('#cardNumber, input[name="cardNumber"]')) {
        let val = e.target.value.replace(/\D/g, '');
        e.target.value = val.replace(/(.{4})/g, '$1 ').trim().substring(0, 19);
    }

    if (e.target.matches('#expiry, input[name="expiry"]')) {
        let val = e.target.value.replace(/\D/g, '');
        if (val.length >= 3) {
            e.target.value = val.substring(0, 2) + '/' + val.substring(2, 4);
        } else {
            e.target.value = val.substring(0, 5);
        }
    }

    if (e.target.matches('#cvv, input[name="cvv"]')) {
        e.target.value = e.target.value.replace(/\D/g, '').substring(0, 3);
    }

    if (e.target.matches('input[name="FullName"]')) {
        let val = e.target.value.replace(/[^a-zA-Zа-яА-ЯёЁ\s-]/g, '');
        val = val.replace(/\s\s+/g, ' ');
        if (e.target.value !== val) {
            e.target.value = val;
        }
    }

    if (e.target.matches('input[name="Email"]')) {
        let val = e.target.value.replace(/[^a-zA-Z0-9@._-]/g, '');
        if (e.target.value !== val) {
            e.target.value = val;
        }
    }

    if (e.target.matches('.val-input')) {
        validateInput(e.target);
    }
});

document.addEventListener('focusout', function (e) {
    if (e.target.matches('.val-input')) {
        validateInput(e.target);
    }
});

document.addEventListener('submit', function (e) {
    const paymentForm = e.target.closest('#paymentForm, form[action*="ProcessPayment"]');
    if (paymentForm) {
        const card = paymentForm.querySelector('#cardNumber, input[name="cardNumber"]');
        const expiry = paymentForm.querySelector('#expiry, input[name="expiry"]');
        const cvv = paymentForm.querySelector('#cvv, input[name="cvv"]');
        let isValid = true;

        if (card && card.value.length !== 19) {
            card.classList.add('is-invalid');
            card.style.borderColor = '#ff4d4d';
            isValid = false;
        } else if (card) {
            card.classList.remove('is-invalid');
            card.style.borderColor = '';
        }

        if (expiry && expiry.value.length !== 5) {
            expiry.classList.add('is-invalid');
            expiry.style.borderColor = '#ff4d4d';
            isValid = false;
        } else if (expiry) {
            expiry.classList.remove('is-invalid');
            expiry.style.borderColor = '';
        }

        if (cvv && cvv.value.length !== 3) {
            cvv.classList.add('is-invalid');
            cvv.style.borderColor = '#ff4d4d';
            isValid = false;
        } else if (cvv) {
            cvv.classList.remove('is-invalid');
            cvv.style.borderColor = '';
        }

        if (!isValid) {
            e.preventDefault();
            e.stopImmediatePropagation();
        }
    }

    const authForm = e.target.closest('#registerForm, #loginForm');
    if (authForm) {
        let isValid = true;
        const inputs = authForm.querySelectorAll('.val-input');

        inputs.forEach(input => {
            validateInput(input);
            if (!input.checkValidity()) {
                isValid = false;
            }
        });

        if (!isValid) {
            e.preventDefault();
            e.stopImmediatePropagation();
        } else {
            showLoader();
        }
    }
});

document.addEventListener('click', function (e) {
    const toggleBtn = e.target.closest('#togglePassword');
    if (toggleBtn) {
        e.preventDefault();
        const container = toggleBtn.closest('.relative');
        const passwordInput = container.querySelector('input[type="password"], input[type="text"]');

        if (passwordInput) {
            const isPassword = passwordInput.type === 'password';
            passwordInput.type = isPassword ? 'text' : 'password';
            const icon = toggleBtn.querySelector('span');
            if (icon) {
                icon.textContent = isPassword ? 'visibility' : 'visibility_off';
            }
        }
    }

    const link = e.target.closest('a');
    if (link) {
        const href = link.getAttribute('href');
        const target = link.getAttribute('target');
        const isHash = href && href.startsWith('#');
        const isJs = href && href.startsWith('javascript:');
        const isDownload = link.hasAttribute('download');
        const isNewTab = target === '_blank';
        const isNoLoader = link.classList.contains('no-loader');

        if (href && !isHash && !isJs && !isDownload && !isNewTab && !isNoLoader && !e.defaultPrevented) {
            showLoader();
        }
    }
});

function validateInput(input) {
    const container = input.closest('div');
    const errorMsg = container ? container.querySelector('.error-msg') : null;

    if (!input.checkValidity() || input.value.trim() === '') {
        input.classList.remove('border-slate-700', 'focus:border-blue-500', 'border-green-500', 'focus:border-green-500');
        input.classList.add('border-red-500', 'focus:border-red-500');
        if (errorMsg) {
            errorMsg.classList.remove('hidden');
        }
    } else {
        input.classList.remove('border-slate-700', 'focus:border-blue-500', 'border-red-500', 'focus:border-red-500');
        input.classList.add('border-green-500', 'focus:border-green-500');
        if (errorMsg) {
            errorMsg.classList.add('hidden');
        }
    }
}

function showLoader() {
    const loader = document.getElementById('pageLoader');
    if (loader) {
        loader.classList.remove('opacity-0', 'pointer-events-none');
        loader.classList.add('opacity-100', 'pointer-events-auto');
    }
}

function hideLoader() {
    const loader = document.getElementById('pageLoader');
    if (loader) {
        loader.classList.remove('opacity-100', 'pointer-events-auto');
        loader.classList.add('opacity-0', 'pointer-events-none');
    }
}
window.playTrackFromElement = function (element) {
    if (window.BeatlyPlayer && typeof window.BeatlyPlayer.playTrackFromElement === 'function') {
        window.BeatlyPlayer.playTrackFromElement(element);
    }
};

window.playTrack = function (url, title, artist, cover) {
    if (window.BeatlyPlayer && typeof window.BeatlyPlayer.playTrack === 'function') {
        window.BeatlyPlayer.playTrack(url, title, artist, cover);
    }
};

window.togglePlay = function () {
    if (window.BeatlyPlayer && typeof window.BeatlyPlayer.togglePlay === 'function') {
        window.BeatlyPlayer.togglePlay();
    }
};

window.toggleLike = function (trackId, button) {
    if (window.BeatlyPlayer && typeof window.BeatlyPlayer.toggleLike === 'function') {
        window.BeatlyPlayer.toggleLike(trackId, button);
    }
};