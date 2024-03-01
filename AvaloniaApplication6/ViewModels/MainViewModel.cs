using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using ReactiveUI;

namespace AvaloniaApplication6.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly Subject<IEnumerable<Item>> subject = new();
    private int t;

    public MainViewModel()
    {
        var dep = new Service();
        
        FeedCommand = ReactiveCommand.Create(() =>
        {
            subject.OnNext(Enumerable.Range(t, 10).Select(i => new Item(i, dep)));
            t += 10;
        });
        
        subject
            .EditDiff(item => item.Key)
            .AsObservableCache()
            .Connect()
            .Bind(out var collection)
            .Subscribe();

        Collection = collection;
    }

    public ReadOnlyObservableCollection<Item> Collection { get; }

    public ReactiveCommand<Unit, Unit> FeedCommand { get; set; }
}

public class Item : ReactiveObject
{
    private readonly Service svc;
    private string projectedText;
    public int Key { get; }

    public Item(int i, Service svc)
    {
        this.svc = svc;
        Key = i;
        this.WhenAnyValue(x => x.svc.Text)
            .BindTo(this, x => x.ProjectedText);
    }

    public string ProjectedText
    {
        get => projectedText;
        set => this.RaiseAndSetIfChanged(ref projectedText, value);
    }
}

public class Service : ReactiveObject
{
    public Service()
    {
        var observable = Observable
            .Timer(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler).Select(_ => GenerateRandomString(5))
            .Repeat();
        
        observable
            .BindTo(this, x => x.Text);
    }
    private static readonly Random random = new Random();
    private string text;

    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public string Text
    {
        get => text;
        set => this.RaiseAndSetIfChanged(ref text, value);
    }
}
