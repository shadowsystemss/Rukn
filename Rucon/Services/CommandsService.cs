using CommandSystem;
using Microsoft.Extensions.DependencyInjection;
using Rucon.Commands.System;
using Rucon.Models;
using System.Text;

namespace Rucon.Services;

public class CommandsService : ITextCommand
{
    private Stack<ICommand> _stack = new();
    private ICommand? _step = null;
    private bool _silence;

    public Action<string, MessageType>? Output;

    private IServiceProvider _services;

    public CommandsService(IServiceProvider services)
    {
        _services = services;
    }

    public ICommand? Execute() => this;

    public ICommand? Execute(string? text)
    {
        if (Intercept(text)) return _step;

        if (_step is null)
            _step = _services.GetRequiredService<StartCommand>().Execute(text);
        else if (_step is ITextCommand textCommand)
            _step = textCommand.Execute(text);

        Message();

        _step = _step?.Execute();

        if (_step is null)
            _stack.TryPop(out _step);

        return _step;
    }

    public ICommand? Command(string? text)
    {
        if (text is not null && text.Contains(' '))
        {
            List<string> args = Parse(text);
            foreach (string a in args)
                Execute(a);
        }
        else Execute(text);
        return _step;
    }

    private bool Intercept(string? text)
    {
        switch (text)
        {
            case "!q":
                _step = null;
                return true;
            case "!s":
                _silence = !_silence;
                return true;
            case "!clear":
                Push(_services.GetRequiredService<ClearCommand>());
                return false;
            case "!cmds":
                Push(_services.GetRequiredService<CmdsCommand>());
                return false;
            default:
                return false;
        }
    }

    private void Message()
    {
        if (_step is IMessageCommand message && message.Message is not null)
            Message(message.Message, MessageType.Unknown);
    }

    public void Message(string text, MessageType type)
    {
        if (!_silence)
            Output?.Invoke(text, type);
    }

    public ICommand? Push(ICommand? command = null)
    {
        if (_step is not null)
            _stack.Push(_step);
        _step = command;
        return _step;
    }

    private List<string> Parse(string text)
    {
        var args = new List<string>();
        var builder = new StringBuilder();
        StringBuilder? subbuilder = null;
        var current = builder;
        for (int i = 0; i < text.Length; i++)
            switch (text[i])
            {
                case '\\':
                    current.Append(text[++i]);
                    break;
                case '"':
                    if (subbuilder is null)
                    {
                        subbuilder = new StringBuilder();
                        current = subbuilder;
                        break;
                    }
                    builder.Append(subbuilder);
                    subbuilder = null;
                    break;
                case ' ':
                    if (subbuilder is null)
                    {
                        args.Add(builder.ToString());
                        builder.Clear();
                    }
                    else subbuilder.Append(' ');
                    break;

                default:
                    current.Append(text[i]);
                    break;
            }

        if (subbuilder is not null)
            builder.Append(subbuilder);
        if (builder.Length > 0)
            args.Add(builder.ToString());

        return args;
    }
}
