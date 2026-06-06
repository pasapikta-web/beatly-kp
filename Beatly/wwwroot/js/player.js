window.BeatlyPlayer = window.BeatlyPlayer || {
    audio: null,
    isShuffle: false,
    isRepeat: false,
    playlist: [],
    currentIndex: -1,
    isPremium: false,
    currentContextContainer: null
};

window.playTrackFromElement = function (el) {
    if (window.BeatlyPlayer && typeof window.BeatlyPlayer.playTrackFromElement === 'function') {
        window.BeatlyPlayer.playTrackFromElement(el);
    }
};

window.playTrack = function (url, title, artist, cover) {
    if (window.BeatlyPlayer && typeof window.BeatlyPlayer.playTrack === 'function') {
        var originEl = window.event ? window.event.target : null;
        if (originEl) {
            window.BeatlyPlayer.currentContextContainer = originEl.closest('.album-tracks') || originEl.closest('#home-tracks-container') || originEl.closest('.main-tracks') || originEl.closest('tbody') || originEl.closest('.space-y-2') || originEl.closest('main') || originEl.parentElement;
        }
        window.BeatlyPlayer.playTrack(url, title, artist, cover, false);
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
        if (!player.audio) {
            player.audio = document.getElementById('global-audio') || document.getElementById('main-audio');
            if (!player.audio) {
                player.audio = document.createElement('audio');
                player.audio.id = 'global-audio';
                player.audio.setAttribute('data-turbo-permanent', '');
                document.body.appendChild(player.audio);
            }
        }

        player.audio.removeEventListener('timeupdate', player.handleTimeUpdate);
        player.audio.addEventListener('timeupdate', player.handleTimeUpdate);
        player.audio.removeEventListener('loadedmetadata', player.handleLoadedMetadata);
        player.audio.addEventListener('loadedmetadata', player.handleLoadedMetadata);
        player.audio.removeEventListener('ended', player.handleEnded);
        player.audio.addEventListener('ended', player.handleEnded);
        player.audio.removeEventListener('play', player.handlePlayState);
        player.audio.addEventListener('play', player.handlePlayState);
        player.audio.removeEventListener('pause', player.handlePauseState);
        player.audio.addEventListener('pause', player.handlePauseState);

        var container = document.getElementById('audio-player-container') || document.getElementById('player-container');
        if (container) {
            player.isPremium = container.getAttribute('data-is-premium') === 'true';
        }

        player.updatePlayIcon(player.audio && !player.audio.paused);
        player.syncRowHighlighting();
    };

    player.normalizeUrl = function (u) {
        if (!u) return "";
        var clean = String(u);
        if (clean.startsWith('http')) {
            try {
                clean = new URL(clean).pathname;
            } catch (e) {
                clean = clean.replace(window.location.origin, '');
            }
        }
        clean = clean.replace('wwwroot/', '').replace('wwwroot\\', '').replace(/\\/g, '/');
        if (!clean.startsWith('/')) clean = '/' + clean;
        return decodeURIComponent(clean);
    };

    player.handleTimeUpdate = function () {
        if (player.audio && player.audio.duration) {
            var current = player.audio.currentTime;
            var duration = player.audio.duration;
            var progress = (current / duration) * 100;
            
            var progressBar = document.getElementById('progress-bar') || document.querySelector('.progress-slider');
            if (progressBar && document.activeElement !== progressBar && !window.BeatlyPlayer.isDraggingSlider) {
                progressBar.value = progress;
            }

            var playerProgress = document.getElementById('player-progress');
            if (playerProgress) {
                playerProgress.style.width = progress + '%';
            }

            var currentTimeEl = document.getElementById('current-time') || document.querySelector('.current-time') || document.getElementById('player-current-time');
            if (currentTimeEl) {
                currentTimeEl.innerText = player.formatTime(current);
            }
        }
    };

    player.handleLoadedMetadata = function () {
        if (player.audio) {
            var durationTimeEl = document.getElementById('duration-time') || document.querySelector('.total-time') || document.getElementById('player-duration');
            if (durationTimeEl) {
                durationTimeEl.innerText = player.formatTime(player.audio.duration);
            }
        }
    };

    player.handleEnded = function () {
        if (player.isRepeat && player.isPremium) {
            player.audio.currentTime = 0;
            player.audio.play().catch(function (e) { });
        } else {
            player.changeTrack(1);
        }
    };

    player.handlePlayState = function () {
        player.updatePlayIcon(true);
        player.syncRowHighlighting();
    };

    player.handlePauseState = function () {
        player.updatePlayIcon(false);
        player.syncRowHighlighting();
    };

    player.formatTime = function (seconds) {
        if (!seconds || isNaN(seconds)) return "0:00";
        var min = Math.floor(seconds / 60);
        var sec = Math.floor(seconds % 60);
        return min + ':' + (sec < 10 ? '0' : '') + sec;
    };

    player.updatePlaylist = function (contextContainer) {
        if (window.serverPlaylist && window.serverPlaylist.length > 0) {
            player.playlist = window.serverPlaylist.map(function (track) {
                return {
                    url: player.normalizeUrl(track.AudioUrl || track.url || track.Url),
                    title: track.Title || track.title || 'Неизвестный трек',
                    artist: track.Artist || track.artist || 'Исполнитель',
                    cover: player.normalizeUrl(track.CoverUrl || track.cover || track.Cover)
                };
            });
            return;
        }

        var searchContainer = document;
        if (contextContainer) {
            var possibleContainer = contextContainer.closest('.album-tracks') || contextContainer.closest('#home-tracks-container') || contextContainer.closest('.main-tracks') || contextContainer.closest('tbody') || contextContainer.closest('.space-y-2') || contextContainer.closest('main');
            if (possibleContainer && possibleContainer.querySelectorAll('.track-row, [data-audio-url]').length > 1) {
                searchContainer = possibleContainer;
                player.currentContextContainer = possibleContainer;
            } else {
                searchContainer = document;
                player.currentContextContainer = null;
            }
        } else if (player.currentContextContainer) {
            searchContainer = player.currentContextContainer;
        }

        var trackElements = searchContainer.querySelectorAll('.track-row, .track-item, [data-audio-url], [onclick*="playTrack"]');
        if ((!trackElements || trackElements.length <= 1) && searchContainer !== document) {
            searchContainer = document;
            trackElements = document.querySelectorAll('.track-row, .track-item, [data-audio-url], [onclick*="playTrack"]');
        }

        var newPlaylist = [];
        var seenUrls = new Set();

        if (trackElements) {
            trackElements.forEach(function (el) {
                if (el.closest('#audio-player-container') || el.closest('#player-container')) return;

                var url = el.getAttribute('data-audio-url') || el.getAttribute('data-src') || el.getAttribute('data-audio');
                var title = el.getAttribute('data-title');
                var artist = el.getAttribute('data-artist');
                var cover = el.getAttribute('data-cover');

                if (!url) {
                    var onclickStr = el.getAttribute('onclick');
                    if (onclickStr && onclickStr.includes('playTrack')) {
                        var cleanStr = onclickStr.replace(/&quot;/g, '"');
                        var matchArgs = cleanStr.match(/playTrack\((.*?)\)/);
                        if (matchArgs && matchArgs[1]) {
                            var args = matchArgs[1].split(',').map(function (arg) {
                                return arg.trim().replace(/^['"]|['"]$/g, '');
                            });
                            url = args[0];
                            if (args.length >= 2) title = args[1];
                            if (args.length >= 3) artist = args[2];
                            if (args.length >= 4) cover = args[3];
                        }
                    }
                }

                if (url) {
                    var normUrl = player.normalizeUrl(url);
                    if (!seenUrls.has(normUrl)) {
                        seenUrls.add(normUrl);
                        newPlaylist.push({
                            url: normUrl,
                            title: title || 'Неизвестный трек',
                            artist: artist || 'Исполнитель',
                            cover: player.normalizeUrl(cover || '/uploads/covers/default.png')
                        });
                    }
                }
            });
        }

        if (newPlaylist.length > 0) {
            player.playlist = newPlaylist;
        }
    };

    player.playTrackFromElement = function (el) {
        if (el) {
            var possibleContainer = el.closest('.album-tracks') || el.closest('#home-tracks-container') || el.closest('.main-tracks') || el.closest('tbody') || el.closest('.space-y-2') || el.closest('main');
            if (possibleContainer && possibleContainer.querySelectorAll('.track-row, [data-audio-url]').length > 1) {
                player.currentContextContainer = possibleContainer;
            } else {
                player.currentContextContainer = null;
            }
        }
        player.updatePlaylist(el);
        var url = el.getAttribute('data-audio-url') || el.getAttribute('data-src') || el.getAttribute('data-audio');
        var title = el.getAttribute('data-title');
        var artist = el.getAttribute('data-artist');
        var cover = el.getAttribute('data-cover');

        if (!url) {
            var onclickStr = el.getAttribute('onclick');
            if (onclickStr && onclickStr.includes('playTrack')) {
                var cleanStr = onclickStr.replace(/&quot;/g, '"');
                var matchArgs = cleanStr.match(/playTrack\((.*?)\)/);
                if (matchArgs && matchArgs[1]) {
                    var args = matchArgs[1].split(',').map(function (arg) {
                        return arg.trim().replace(/^['"]|['"]$/g, '');
                    });
                    url = args[0];
                    if (args.length >= 2) title = args[1];
                    if (args.length >= 3) artist = args[2];
                    if (args.length >= 4) cover = args[3];
                }
            }
        }

        player.playTrack(url, title, artist, cover, true);
    };

    player.playTrack = function (url, title, artist, cover, skipUpdate) {
        player.init();
        if (!url) return;

        var container = document.getElementById('audio-player-container') || document.getElementById('player-container');
        if (container) {
            container.classList.remove('hidden');
            container.style.setProperty('display', 'flex', 'important');
        }

        if (!skipUpdate) {
            player.updatePlaylist();
        }

        var normalizedUrl = player.normalizeUrl(url);
        var currentSrc = "";
        if (player.audio.src) {
            currentSrc = player.normalizeUrl(player.audio.src);
        }

        if (currentSrc === normalizedUrl) {
            if (player.audio.paused) {
                player.audio.play().catch(function (e) { });
            } else {
                player.audio.pause();
            }
            return;
        }

        player.audio.src = normalizedUrl;
        player.audio.play().catch(function (e) { });

        var playerTitle = document.getElementById('player-title') || document.getElementById('player-track-title');
        var playerArtist = document.getElementById('player-artist') || document.getElementById('player-track-artist');
        var playerCover = document.getElementById('player-cover') || document.getElementById('player-track-cover');

        if (playerTitle) playerTitle.innerText = title && title !== 'undefined' ? title : "Неизвестный трек";
        if (playerArtist) playerArtist.innerText = artist && artist !== 'undefined' ? artist : "Неизвестный исполнитель";
        if (playerCover) {
            var cleanCover = cover && cover !== 'undefined' ? cover : "/uploads/covers/default.png";
            playerCover.src = player.normalizeUrl(cleanCover);
            playerCover.style.display = "block";
        }

        player.currentIndex = player.playlist.findIndex(function (t) {
            return t.url === normalizedUrl || t.url.includes(normalizedUrl) || normalizedUrl.includes(t.url);
        });

        if (player.currentIndex === -1) {
            player.playlist.push({
                url: normalizedUrl,
                title: title || 'Неизвестный трек',
                artist: artist || 'Исполнитель',
                cover: player.normalizeUrl(cover || '/uploads/covers/default.png')
            });
            player.currentIndex = player.playlist.length - 1;
        }

        player.syncRowHighlighting();
    };

    player.syncRowHighlighting = function () {
        if (!player.audio || !player.audio.src) return;
        var currentSrc = player.normalizeUrl(player.audio.src);
        var isPaused = player.audio.paused;

        var rows = document.querySelectorAll('.track-row, .track-item');
        rows.forEach(function (row) {
            var rowUrl = player.normalizeUrl(row.getAttribute('data-audio-url') || row.getAttribute('data-src') || row.getAttribute('data-audio'));
            var rowPlayIcon = row.querySelector('.fa-play, .fa-pause');

            if (rowUrl === currentSrc) {
                row.classList.add('bg-white/10', 'text-blue-500');
                if (rowPlayIcon) {
                    if (isPaused) {
                        rowPlayIcon.classList.remove('fa-pause');
                        rowPlayIcon.classList.add('fa-play');
                    } else {
                        rowPlayIcon.classList.remove('fa-play');
                        rowPlayIcon.classList.add('fa-pause');
                    }
                }
            } else {
                row.classList.remove('bg-white/10', 'text-blue-500');
                if (rowPlayIcon) {
                    rowPlayIcon.classList.remove('fa-pause');
                    rowPlayIcon.classList.add('fa-play');
                }
            }
        });
    };

    player.updatePlayIcon = function (isPlaying) {
        var playToggleBtn = document.getElementById('player-play-toggle');
        if (playToggleBtn) {
            var icon = playToggleBtn.querySelector('i');
            if (icon) {
                if (isPlaying) {
                    icon.className = 'fa-solid fa-pause';
                } else {
                    icon.className = 'fa-solid fa-play ml-0.5';
                }
            }
        }

        var playBtns = document.querySelectorAll('#play-btn, #play-pause-btn, .main-play');
        playBtns.forEach(function (btn) {
            if (isPlaying) {
                btn.innerHTML = '<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="6" y="4" width="4" height="16"></rect><rect x="14" y="4" width="4" height="16"></rect></svg>';
            } else {
                btn.innerHTML = '<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="5 3 19 12 5 21 5 3"></polygon></svg>';
            }
        });

        var playIcons = document.querySelectorAll('#play-icon');
        playIcons.forEach(function (icon) {
            icon.innerText = isPlaying ? 'pause' : 'play_arrow';
        });

        var globalPlayButton = document.getElementById("global-play-btn");
        if (globalPlayButton) {
            globalPlayButton.innerHTML = isPlaying ? '<i class="bi bi-pause-fill"></i>' : '<i class="bi bi-play-fill"></i>';
        }
    };

    player.togglePlay = function () {
        player.init();
        if (!player.audio) return;

        if (!player.audio.src || player.audio.src.endsWith(window.location.host + '/') || player.audio.src === window.location.href) {
            player.updatePlaylist();
            if (player.playlist.length > 0) {
                player.currentIndex = 0;
                var t = player.playlist[0];
                player.playTrack(t.url, t.title, t.artist, t.cover, true);
            }
            return;
        }

        if (player.audio.paused) {
            player.audio.play().catch(function (e) { });
        } else {
            player.audio.pause();
        }
    };

    player.changeTrack = function (direction) {
        player.init();
        player.updatePlaylist();
        
        if (player.playlist.length === 0) return;

        if (player.audio && player.audio.src) {
            var currentSrc = player.normalizeUrl(player.audio.src);
            var foundIndex = player.playlist.findIndex(function (t) {
                return t.url === currentSrc || currentSrc.includes(t.url) || t.url.includes(currentSrc);
            });
            if (foundIndex !== -1) {
                player.currentIndex = foundIndex;
            }
        }

        if (player.isShuffle && player.isPremium) {
            var nextIndex = Math.floor(Math.random() * player.playlist.length);
            while (nextIndex === player.currentIndex && player.playlist.length > 1) {
                nextIndex = Math.floor(Math.random() * player.playlist.length);
            }
            player.currentIndex = nextIndex;
        } else {
            player.currentIndex += direction;
            if (player.currentIndex >= player.playlist.length) player.currentIndex = 0;
            if (player.currentIndex < 0) player.currentIndex = player.playlist.length - 1;
        }

        var track = player.playlist[player.currentIndex];
        if (track) {
            player.playTrack(track.url, track.title, track.artist, track.cover, true);
        }
    };

    player.toggleShuffle = function () {
        var shuffleBtn = document.getElementById('shuffle-btn') || document.getElementById('player-shuffle');
        if (!player.isPremium) {
            alert('Эта функция доступна только для подписчиков Beatly Premium.');
            return;
        }
        player.isShuffle = !player.isShuffle;
        if (shuffleBtn) {
            shuffleBtn.classList.toggle('text-[#499BED]', player.isShuffle);
            shuffleBtn.classList.toggle('text-gray-400', !player.isShuffle);
        }
    };

    player.toggleRepeat = function () {
        var repeatBtn = document.getElementById('repeat-btn') || document.getElementById('loop-btn') || document.getElementById('player-repeat');
        if (!player.isPremium) {
            alert('Эта функция доступна только для подписчиков Beatly Premium.');
            return;
        }
        player.isRepeat = !player.isRepeat;
        if (player.audio) player.audio.loop = player.isRepeat;
        if (repeatBtn) {
            repeatBtn.classList.toggle('text-[#499BED]', player.isRepeat);
            repeatBtn.classList.toggle('text-gray-400', !player.isRepeat);
        }
    };

    if (!window._beatlyEventsBound) {
        document.addEventListener('click', function (e) {
            var playBtn = e.target.closest('#play-btn, #play-pause-btn, .main-play, #player-play-toggle, .bi-play-fill, .bi-pause-fill, .fa-play, .fa-pause');
            if (playBtn) { player.togglePlay(); return; }

            var nextBtn = e.target.closest('#next-btn, #player-next, .player-next, [id*="next"], [class*="next"], .bi-skip-forward-fill, .fa-forward-step, .fa-step-forward');
            if (nextBtn) { player.changeTrack(1); return; }

            var prevBtn = e.target.closest('#prev-btn, #player-prev, .player-prev, [id*="prev"], [class*="prev"], .bi-skip-backward-fill, .fa-backward-step, .fa-step-backward');
            if (prevBtn) { player.changeTrack(-1); return; }

            var shuffleBtn = e.target.closest('#shuffle-btn, #player-shuffle');
            if (shuffleBtn) { e.stopPropagation(); player.toggleShuffle(); return; }

            var repeatBtn = e.target.closest('#repeat-btn, #loop-btn, #player-repeat');
            if (repeatBtn) { e.stopPropagation(); player.toggleRepeat(); return; }

            var trackRow = e.target.closest('.track-row, .track-item');
            if (trackRow) {
                var clickedControl = e.target.closest('#play-btn, #play-pause-btn, .main-play, #player-play-toggle, #next-btn, #player-next, #prev-btn, #player-prev, #shuffle-btn, #player-shuffle, #repeat-btn, #loop-btn, #player-repeat');
                if (!clickedControl) {
                    player.playTrackFromElement(trackRow);
                }
                return;
            }

            var progressContainer = e.target.closest('#progress-container');
            if (progressContainer && player.audio && player.audio.duration) {
                var rect = progressContainer.getBoundingClientRect();
                var clickX = e.clientX - rect.left;
                var width = rect.width;
                player.audio.currentTime = (clickX / width) * player.audio.duration;
                return;
            }
        });

        document.addEventListener('input', function (e) {
            if (e.target.matches('#progress-bar') || e.target.matches('.progress-slider')) {
                window.BeatlyPlayer.isDraggingSlider = true;
            }
            if (e.target.matches('#volume-slider') || e.target.matches('.volume-slider')) {
                if (player.audio) {
                    player.audio.volume = e.target.value;
                }
            }
        });

        document.addEventListener('change', function (e) {
            if (e.target.matches('#progress-bar') || e.target.matches('.progress-slider')) {
                window.BeatlyPlayer.isDraggingSlider = false;
                if (player.audio && player.audio.duration) {
                    player.audio.currentTime = (e.target.value * player.audio.duration) / 100;
                }
            }
        });

        window.toggleLike = function (trackId, button) {
            var token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            fetch('/Home/ToggleLike?id=' + trackId, {
                method: 'POST',
                headers: { 'RequestVerificationToken': token }
            })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (data.success) {
                        var icon = button.querySelector('svg') || button.querySelector('.material-symbols-outlined');
                        if (icon) {
                            if (data.isLiked) {
                                if (icon.tagName.toLowerCase() === 'svg') {
                                    icon.setAttribute('fill', 'currentColor');
                                    icon.classList.add('text-red-500');
                                } else {
                                    icon.classList.add('text-red-500', 'fill-1');
                                    icon.classList.remove('text-gray-400');
                                }
                            } else {
                                if (icon.tagName.toLowerCase() === 'svg') {
                                    icon.setAttribute('fill', 'none');
                                    icon.classList.remove('text-red-500');
                                } else {
                                    icon.classList.remove('text-red-500', 'fill-1');
                                    icon.classList.add('text-gray-400');
                                }
                            }
                        }
                    }
                })
                .catch(function (error) { });
        };

        window._beatlyEventsBound = true;
    }

    if (document.readyState !== 'loading') {
        player.init();
    }
    document.addEventListener("DOMContentLoaded", player.init);
    document.addEventListener("turbo:load", function () {
        player.init();
    });

})(window.BeatlyPlayer);