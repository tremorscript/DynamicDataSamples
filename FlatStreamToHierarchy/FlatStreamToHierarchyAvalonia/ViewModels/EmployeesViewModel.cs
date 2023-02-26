using DynamicData;
using FlatStreamToHierarchyAvalonia.Models;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace FlatStreamToHierarchyAvalonia.ViewModels;

public class EmployeesViewModel
{
    private readonly EmployeeService _employeeService;
    private readonly ReadOnlyObservableCollection<EmployeeViewModel> _employeeViewModels;
    private readonly IDisposable _cleanUp;

    public EmployeesViewModel(EmployeeService employeeService)
    {
        _employeeService = employeeService;

        bool DefaultPredicate(Node<Employee, int> node) => node.IsRoot;

        //transform the data to a full nested tree
        //then transform into a fully recursive view model
        _cleanUp = employeeService.Employees.Connect()
            .TransformToTree(employee => employee.BossId, Observable.Return((Func<Node<Employee, int>, bool>)DefaultPredicate))
            .Transform(node => new EmployeeViewModel(node, Promote, Sack))
            .Bind(out _employeeViewModels)
            .DisposeMany()
            .Subscribe();
    }

    public ReadOnlyObservableCollection<EmployeeViewModel> EmployeeViewModels => _employeeViewModels;

    public void Dispose()
    {
        _cleanUp.Dispose();
    }

    private void Promote(EmployeeViewModel viewModel)
    {
        //Checks to see if it the ultimate boss
        if (!viewModel.Parent.HasValue) return;
        _employeeService.Promote(viewModel.Dto, viewModel.Parent.Value.BossId);
    }

    private void Sack(EmployeeViewModel viewModel)
    {
        _employeeService.Sack(viewModel.Dto);
    }
}