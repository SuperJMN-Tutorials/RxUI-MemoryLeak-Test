using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
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
        var dep = new Dependency();
        
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
    private readonly Dependency dep;
    public int Key { get; }

    public Item(int i, Dependency dep)
    {
        this.dep = dep;
        Key = i;
        this.WhenAnyValue(item => item.dep.Prop)
            .BindTo(this, x => x.Prop);
    }

    public string Prop { get; set; }
}

public class Dependency : ReactiveObject
{
    public string Prop { get; set; } = "salute";
}
