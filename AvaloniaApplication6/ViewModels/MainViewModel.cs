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
    private readonly Subject<IEnumerable<Item>> itemListsObs = new();
    private int t;

    public MainViewModel()
    {
        // Each time we execute this command, we create a batch of Items that replaces the previous batch.
        CreateNewItems = ReactiveCommand.Create(() =>
        {
            itemListsObs.OnNext(Enumerable.Range(t, 10).Select(i => new Item(i, Singleton.Service)));
            t += 10;
        });
        
        // itemListsObs is fed by the command
        itemListsObs
            .EditDiff(item => item.Key)
            .AsObservableCache()
            .Connect()
            .Bind(out var collection)
            .Subscribe();

        Items = collection;
    }

    public ReadOnlyObservableCollection<Item> Items { get; }

    public ReactiveCommand<Unit, Unit> CreateNewItems { get; set; }
}

public class Item : ReactiveObject
{
    private string? projectedText;

    public Item(int i, Service service)
    {
        Key = i;
        Service = service;
        
        // Here, we subscribe to a "foreign" object whose lifetime isn't controlled by the Item.
        this.WhenAnyValue(x => x.Service.Text)
            .BindTo(this, x => x.ProjectedText);
    }

    public Service Service { get; }
    
    public int Key { get; }
    
    public string? ProjectedText
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
    
    private string? text;

    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }

    public string? Text
    {
        get => text;
        set => this.RaiseAndSetIfChanged(ref text, value);
    }
}

public static class Singleton
{
    public static readonly Service Service = new();
}