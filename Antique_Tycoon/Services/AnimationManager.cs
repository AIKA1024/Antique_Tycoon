using System.Collections.Generic;
using System.Threading.Tasks;
using Antique_Tycoon.Behaviors;
using Antique_Tycoon.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace Antique_Tycoon.Services;

public class AnimationManager
{
    private readonly Dictionary<string, Task> _animationTasks = [];
    
    public bool HasAnimationRunning => _animationTasks.Count > 0; 

    public async Task StartPlayerMoveAnimation(Player player, List<string> path, string token)
    {
        var animationMessage = WeakReferenceMessenger.Default.Send(new StartPlayerMoveAnimation(player, path));
        _animationTasks[token] = await animationMessage.Response;
        await _animationTasks[token];
        _animationTasks.Remove(token);
    }

    public Task WaitAnimation(string token)
    {
        _animationTasks.TryGetValue(token, out var animationTask);
        return animationTask ?? Task.CompletedTask;
    }
}