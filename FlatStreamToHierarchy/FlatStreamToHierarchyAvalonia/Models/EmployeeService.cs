using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlatStreamToHierarchyAvalonia.Models;

public class EmployeeService
{
    // A Source Cache with the Id of the employee as the key.
    private readonly SourceCache<Employee, int> _employees = new SourceCache<Employee, int>(x => x.Id);

    public EmployeeService()
    {
        _employees.AddOrUpdate(CreateEmployees(25000));
    }

    //     A cache for observing and querying in memory data. With additional data access
    //     operators.
    public IObservableCache<Employee, int> Employees => _employees.AsObservableCache();

    public void Promote(Employee promoted, int newBoss)
    {
        // in the real world, go to service then update the cache.

        // update the cache with the employee.
        _employees.AddOrUpdate(new Employee(promoted.Id, promoted.Name, newBoss));
    }

    public void Sack(Employee sackEmp)
    {
        // in the real world, go to service then updated the cache.

        _employees.Edit(updater =>
        {
            // assign new boss to the workers of the sacked employee.
            // Assign the boss of the sacker employee to the workers.
            var workersWithNewBoss = updater.Items
                                .Where(emp => emp.BossId == sackEmp.Id)
                                .Select(dto => new Employee(dto.Id, dto.Name, sackEmp.BossId))
                                .ToArray();

            updater.AddOrUpdate(workersWithNewBoss);

            //get rid of the existing person
            updater.Remove(sackEmp.Id);
        });
    }

    /// <summary>
    /// Creates 2500 employees on page load.
    /// </summary>
    /// <param name="numberToLoad"></param>
    /// <returns></returns>
    private IEnumerable<Employee> CreateEmployees(int numberToLoad)
    {
        var random = new Random();

        return Enumerable.Range(1, numberToLoad)
            .Select(i =>
            {
                //Select a random boss for an employee.
                var boss = i % 1000 == 0 ? 0 : random.Next(0, i);
                return new Employee(i, $"Person {i}", boss);
            });
    }
}