using System;
using System.Collections.Generic;
using LibVLCSharp.Shared;

namespace Antique_Tycoon.Services;

public class SoundService : IDisposable
{
    private readonly LibVLC _libVlc;
    
    // --- 两个独立的播放器 ---
    private readonly MediaPlayer _sfxPlayer; // 轨道1：音效
    private readonly MediaPlayer _bgmPlayer; // 轨道2：背景音乐

    // 缓存 Media 对象，避免重复加载
    private readonly Dictionary<string, Media> _mediaCache = new();

    public SoundService(LibVLC libVlc)
    {
        _libVlc = libVlc;
        
        // 初始化两个独立的播放器
        _sfxPlayer = new MediaPlayer(_libVlc);
        _bgmPlayer = new MediaPlayer(_libVlc);
        
        // 默认音量设置 (BGM通常要比音效小一点，不然吵)
        _bgmPlayer.Volume = 60; 
        _sfxPlayer.Volume = 100;
    }

    // 1. 播放音效 (SFX) - 简单粗暴逻辑：有新的来就切掉旧的
    public void PlaySoundEffect(string path)
    {
        try
        {
            var media = GetOrLoadMedia(path);
            
            // 因为 _sfxPlayer 和 _bgmPlayer 是分开的
            // 所以这里切歌只会打断上一次的“点击声”，不会影响 BGM
            _sfxPlayer.Media = media;
            _sfxPlayer.Stop(); // 停止上一个音效
            _sfxPlayer.Play(); // 播放新的
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SFX Error: {ex.Message}");
        }
    }

    // 2. 播放背景音乐 (BGM) - 独立逻辑，支持循环
    public void PlayBackgroundMusic(string path, bool repeat = true)
    {
        try
        {
            // 如果已经在放这一首了，就别重新开始了，直接返回
            // (除非你想强制重头放，可以把这个判断去掉)
            if (_bgmPlayer.Media?.Mrl.EndsWith(path) == true && _bgmPlayer.IsPlaying)
            {
                return; 
            }

            var media = GetOrLoadMedia(path);

            if (repeat)
            {
                // LibVLC 的黑魔法：添加参数让它无限循环
                // "input-repeat=65535" 表示重复播放很多次
                media.AddOption(":input-repeat=65535");
            }

            _bgmPlayer.Media = media;
            _bgmPlayer.Play();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BGM Error: {ex.Message}");
        }
    }

    public void StopBackgroundMusic()
    {
        _bgmPlayer.Stop();
    }

    // 辅助方法：统一管理 Media 缓存
    private Media GetOrLoadMedia(string path)
    {
        if (!_mediaCache.TryGetValue(path, out var media))
        {
            // FromType.FromPath 适用于本地文件
            media = new Media(_libVlc, path, FromType.FromPath);
            media.Parse(); // 预解析
            _mediaCache[path] = media;
        }
        return media;
    }
    
    // 调节音量
    public void SetMusicVolume(int volume) => _bgmPlayer.Volume = volume;
    public void SetSfxVolume(int volume) => _sfxPlayer.Volume = volume;

    public void Dispose()
    {
        _sfxPlayer.Dispose();
        _bgmPlayer.Dispose();
        foreach (var m in _mediaCache.Values) m.Dispose();
    }
}