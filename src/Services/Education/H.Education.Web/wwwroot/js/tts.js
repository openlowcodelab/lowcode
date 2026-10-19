// 听力与对话练习的朗读：直接用浏览器 Web Speech 合成，
// 不需要后端接 TTS，也不用把音频文件存到文件应用里。
let voices = [];

function refreshVoices() {
    if (typeof speechSynthesis === 'undefined') return;
    voices = speechSynthesis.getVoices() || [];
}

if (typeof speechSynthesis !== 'undefined') {
    refreshVoices();
    speechSynthesis.onvoiceschanged = refreshVoices;
}

function isEnglish(text) {
    return !/[\u4e00-\u9fa5]/.test(text);
}

function pickVoice(lang) {
    const prefix = lang.slice(0, 2);
    return voices.find(v => (v.lang || '').toLowerCase().startsWith(prefix.toLowerCase())) || null;
}

export function isSupported() {
    return typeof speechSynthesis !== 'undefined';
}

export function speak(text, rate) {
    if (!isSupported() || !text) return false;

    // 连续点播放时先掐掉上一段，否则语音会排队叠在一起
    speechSynthesis.cancel();

    const lang = isEnglish(text) ? 'en-US' : 'zh-CN';
    const utterance = new SpeechSynthesisUtterance(text);
    utterance.lang = lang;
    utterance.rate = rate > 0 ? rate : 0.9;

    const voice = pickVoice(lang);
    if (voice) utterance.voice = voice;

    speechSynthesis.speak(utterance);
    return true;
}

export function stop() {
    if (isSupported()) speechSynthesis.cancel();
}
