using DynamicData;
using DynamicData.Kernel;
using FlatStreamToHierarchyAvalonia.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace FlatStreamToHierarchyAvalonia.ViewModels;

public class EmployeeViewModel : ViewModelBase, IDisposable, IEquatable<EmployeeViewModel>
{
    private readonly IDisposable _cleanUp;
    private bool _isExpanded;
    private bool _isSelected;
    private string _employeeCountText;
    private ReadOnlyObservableCollection<EmployeeViewModel> _inferiors;

    public EmployeeViewModel(Node<Employee, int> node,
        Action<EmployeeViewModel> promoteAction,
        Action<EmployeeViewModel> sackAction,
        EmployeeViewModel parent = null)
    {
        Id = node.Key;
        Name = node.Item.Name;
        Depth = node.Depth;
        Parent = parent;
        BossId = node.Item.BossId;
        Dto = node.Item;

        var canPromote = Observable.Return(Parent.HasValue);

        PromoteCommand = ReactiveCommand.Create(() => promoteAction(this), canPromote);
        SackCommand = ReactiveCommand.Create(() => sackAction(this));

        // Wrap loader for the nested view model inside a lazy so we can control when it is invoked
        var childrenLoader = new Lazy<IDisposable>(() => node.Children.Connect()
                            .Transform(e => new EmployeeViewModel(e, promoteAction, sackAction, this))
                            .Bind(out _inferiors)
                            .DisposeMany()
                            .Subscribe());

        // return true when the children should be loaded
        // (i.e. if current node is a root, otherwise when the parent expands)
        var shouldExpand = node.IsRoot
            ? Observable.Return(true)
            : Parent.Value.WhenAnyValue(x => x.IsExpanded);

        //wire the observable
        var expander = shouldExpand
                .Where(isExpanded => isExpanded)
                .Take(1)
                .Subscribe(_ =>
                {
                    //force lazy loading
                    var x = childrenLoader.Value;
                });

        //create some display text based on the number of employees
        var employeesCount = node.Children.CountChanged
            .Select(count =>
            {
                if (count == 0)
                    return "I am a at rock bottom";

                return count == 1
                   ? "1 person reports to me"
                   : $"{count} people reports to me";
            }).Subscribe(text => EmployeeCountText = text);

        _cleanUp = Disposable.Create(() =>
        {
            expander.Dispose();
            employeesCount.Dispose();
            if (childrenLoader.IsValueCreated)
                childrenLoader.Value.Dispose();
        });
    }

    public int Id { get; }

    public string Name { get; }

    public int Depth { get; }

    public int BossId { get; }

    public Employee Dto { get; }

    public Optional<EmployeeViewModel> Parent { get; }

    public ReadOnlyObservableCollection<EmployeeViewModel> Inferiors => _inferiors;

    public ReactiveCommand<Unit, Unit> PromoteCommand { get; }

    public ReactiveCommand<Unit, Unit> SackCommand { get; }

    public string EmployeeCountText
    {
        get => _employeeCountText;
        set => this.RaiseAndSetIfChanged(ref _employeeCountText, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    #region Equality Members

    public static bool operator ==(EmployeeViewModel left, EmployeeViewModel right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(EmployeeViewModel left, EmployeeViewModel right)
    {
        return !Equals(left, right);
    }

    public bool Equals(EmployeeViewModel other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((EmployeeViewModel)obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }

    #endregion Equality Members

    public void Dispose()
    {
        _cleanUp.Dispose();
    }
}