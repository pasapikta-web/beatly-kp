window.BeatlyPlayer = window.BeatlyPlayer || {
    audio: null,
    isShuffle: false,
    isRepeat: false,
    playlist: [],
    currentIndex: -1,
    isPremium: false,
    elements: {},
    isInitialized: false
};

window.playTrackFromElement = function (el) {
    if (window.BeatlyPlayer && typeof window.BeatlyPlayer.playTrackFromElement === 'function') {
        window.BeatlyPlayer.playTrackFromElement(el);
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

(function (player) {
    player.init = function () {
        if (player.isInitialized) return;

        if (!player.audio) {
            player.audio = document.getElementById('main-audio');
            if (!player.audio) {
                player.audio = document.createElement('audio');
                player.audio.id = 'main-audio';
                player.audio.setAttribute('data-turbo-permanent', '');
                document.body.appendChild(player.audio);
            }
        }

        player.elements.playBtn = document.getElementById('play-btn');
        player.elements.playPauseBtn = document.getElementById('play-pause-btn') || player.elements.playBtn;
        player.elements.playIcon = document.getElementById('play-icon');
        player.elements.progressBar = document.getElementById('progress-bar') || document.querySelector('.progress-slider');
        player.elements.volumeSlider = document.getElementById('volume-slider') || document.querySelector('.volume-slider');
        player.elements.currentTimeEl = document.getElementById('current-time') || document.querySelector('.current-time');
        player.elements.durationTimeEl = document.getElementById('duration-time') || document.querySelector('.total-time');
        player.elements.shuffleBtn = document.getElementById('shuffle-btn');
        player.elements.repeatBtn = document.getElementById('repeat-btn') || document.getElementById('loop-btn');
        player.elements.prevBtn = document.getElementById('prev-btn');
        player.elements.nextBtn = document.getElementById('next-btn');

        var container = document.getElementById('audio-player-container') || document.getElementById('player-container');
        if (container) {
            player.isPremium = container.getAttribute('data-is-premium') === 'true';
        }

        player.audio.removeEventListener('timeupdate', player.handleTimeUpdate);
        player.audio.addEventListener('timeupdate', player.handleTimeUpdate);

        player.audio.removeEventListener('loadedmetadata', player.handleLoadedMetadata);
        player.audio.addEventListener('loadedmetadata', player.handleLoadedMetadata);

        player.audio.removeEventListener('ended', player.handleEnded);
        player.audio.addEventListener('ended', player.handleEnded);

        if (player.elements.progressBar) {
            player.elements.progressBar.oninput = function () {
                if (player.audio.duration) {
                    player.audio.currentTime = (player.elements.progressBar.value * player.audio.duration) / 100;
                }
            };
        }

        if (player.elements.volumeSlider) {
            player.elements.volumeSlider.oninput = function () {
                player.audio.volume = player.elements.volumeSlider.value / 100;
            };
        }

        if (player.elements.playBtn) player.elements.playBtn.onclick = player.togglePlay;
        if (player.elements.playPauseBtn && player.elements.playPauseBtn !== player.elements.playBtn) {
            player.elements.playPauseBtn.onclick = player.togglePlay;
        }
        if (player.elements.nextBtn) player.elements.nextBtn.onclick = function () { player.changeTrack(1); };
        if (player.elements.prevBtn) player.elements.prevBtn.onclick = function () { player.changeTrack(-1); };

        if (player.elements.shuffleBtn) {
            player.elements.shuffleBtn.classList.toggle('text-[#499BED]', player.isShuffle);
            player.elements.shuffleBtn.onclick = function () {
                if (!player.isPremium) { return player.showPremiumModal(); }
                player.isShuffle = !player.isShuffle;
                player.elements.shuffleBtn.classList.toggle('text-[#499BED]', player.isShuffle);
                player.elements.shuffleBtn.classList.toggle('text-gray-400', !player.isShuffle);
            };
        }

        if (player.elements.repeatBtn) {
            player.elements.repeatBtn.classList.toggle('text-[#499BED]', player.isRepeat);
            player.audio.loop = player.isRepeat;
            player.elements.repeatBtn.onclick = function () {
                if (!player.isPremium) { return player.showPremiumModal(); }
                player.isRepeat = !player.isRepeat;
                player.audio.loop = player.isRepeat;
                player.elements.repeatBtn.classList.toggle('text-[#499BED]', player.isRepeat);
                player.elements.repeatBtn.classList.toggle('text-gray-400', !player.isRepeat);
            };
        }

        player.updatePlaylist();
        player.updatePlayIcon(!player.audio.paused);
        player.isInitialized = true;
    };

    player.handleTimeUpdate = function () {
        if (player.audio && player.audio.duration) {
            var progress = (player.audio.currentTime / player.audio.duration) * 100;
            if (player.elements.progressBar) player.elements.progressBar.value = progress;
            if (player.elements.currentTimeEl) player.elements.currentTimeEl.innerText = player.formatTime(player.audio.currentTime);
        }
    };

    player.handleLoadedMetadata = function () {
        if (player.audio && player.elements.durationTimeEl) {
            player.elements.durationTimeEl.innerText = player.formatTime(player.audio.duration);
        }
    };

    player.handleEnded = function () {
        if (player.isRepeat && player.isPremium) {
            player.audio.play();
        } else {
            player.changeTrack(1);
        }
    };

    player.formatTime = function (seconds) {
        var min = Math.floor(seconds / 60);
        var sec = Math.floor(seconds % 60);
        return min + ':' + (sec < 10 ? '0' : '') + sec;
    };

    player.updatePlaylist = function () {
        var trackElements = document.querySelectorAll('.track-row');
        player.playlist = Array.from(trackElements).map(function (el) {
            return {
                url: el.getAttribute('data-src') || el.getAttribute('data-audio'),
                title: el.getAttribute('data-title'),
                artist: el.getAttribute('data-artist'),
                cover: el.getAttribute('data-cover')
            };
        }).filter(function (item) { return item && item.url; });
    };

    player.playTrackFromElement = function (el) {
        var url = el.getAttribute('data-src') || el.getAttribute('data-audio');
        var title = el.getAttribute('data-title');
        var artist = el.getAttribute('data-artist');
        var cover = el.getAttribute('data-cover');
        player.playTrack(url, title, artist, cover);
    };

    player.playTrack = function (url, title, artist, cover) {
        if (!player.audio) player.init();
        if (!url) return;

        var container = document.getElementById('audio-player-container') || document.getElementById('player-container');
        if (container) {
            container.classList.remove('hidden');
            container.style.setProperty('display', 'flex', 'important');
        }

        player.updatePlaylist();

        var cleanUrl = url.replace('wwwroot/', '').replace('wwwroot\\', '').replace(/\\/g, '/');
        if (!cleanUrl.startsWith('/')) cleanUrl = '/' + cleanUrl;

        var currentSrc = decodeURIComponent(player.audio.src.replace(window.location.origin, ''));
        if (currentSrc === cleanUrl) {
            player.togglePlay();
            return;
        }

        player.audio.src = cleanUrl;
        player.audio.play().catch(function (e) { console.error(e); });

        var playerTitle = document.getElementById('player-title') || document.getElementById('player-track-title');
        var playerArtist = document.getElementById('player-artist') || document.getElementById('player-track-artist');
        var playerCover = document.getElementById('player-cover') || document.getElementById('player-track-cover');

        if (playerTitle) playerTitle.innerText = title && title !== 'undefined' ? title : "Неизвестный трек";
        if (playerArtist) playerArtist.innerText = artist && artist !== 'undefined' ? artist : "Неизвестный исполнитель";
        if (playerCover) {
            var cleanCover = cover && cover !== 'undefined' ? cover : "/uploads/covers/default.png";
            cleanCover = cleanCover.replace('wwwroot/', '').replace('wwwroot\\', '').replace(/\\/g, '/');
            if (!cleanCover.startsWith('/')) cleanCover = '/' + cleanCover;
            playerCover.src = cleanCover;
            playerCover.style.display = "block";
        }

        player.updatePlayIcon(true);
        player.currentIndex = player.playlist.findIndex(function (t) { return t.url === url || t.url.includes(url); });
    };

    player.updatePlayIcon = function (isPlaying) {
        if (player.elements.playIcon) {
            player.elements.playIcon.innerText = isPlaying ? 'pause' : 'play_arrow';
        }
        var globalPlayButton = document.getElementById("global-play-btn");
        if (globalPlayButton) {
            globalPlayButton.innerHTML = isPlaying ? '<i class="bi bi-pause-fill"></i>' : '<i class="bi bi-play-fill"></i>';
        }
    };

    player.togglePlay = function () {
        if (!player.audio) return;
        if (player.audio.paused) {
            player.audio.play().catch(function (e) { console.error(e); });
            player.updatePlayIcon(true);
        } else {
            player.audio.pause();
            player.updatePlayIcon(false);
        }
    };

    player.changeTrack = function (direction) {
        if (player.playlist.length === 0) player.updatePlaylist();
        if (player.playlist.length === 0) return;

        if (player.isShuffle && player.isPremium) {
            player.currentIndex = Math.floor(Math.random() * player.playlist.length);
        } else {
            player.currentIndex += direction;
            if (player.currentIndex >= player.playlist.length) player.currentIndex = 0;
            if (player.currentIndex < 0) player.currentIndex = player.playlist.length - 1;
        }

        var track = player.playlist[player.currentIndex];
        if (track) {
            player.playTrack(track.url, track.title, track.artist, track.cover);
        }
    };

    player.showPremiumModal = function () {
        alert('Эта функция доступна только для подписчиков Beatly Premium.');
    };

    player.toggleLike = function (trackId, button) {
        var token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        fetch('/Home/ToggleLike?id=' + trackId, {
            method: 'POST',
            headers: { 'RequestVerificationToken': token }
        })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data.success) {
                    var icon = button.querySelector('svg');
                    if (icon) {
                        if (data.isLiked) {
                            icon.setAttribute('fill', 'currentColor');
                            icon.classList.add('text-red-500');
                        } else {
                            icon.setAttribute('fill', 'none');
                            icon.classList.remove('text-red-500');
                        }
                    }
                }
            })
            .catch(function (error) { console.error(error); });
    };

    player.playTrackFromElement = player.playTrackFromElement;
    player.toggleLike = player.toggleLike;

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", player.init);
    } else {
        player.init();
    }
    document.addEventListener("turbo:load", player.init);

})(window.BeatlyPlayer);