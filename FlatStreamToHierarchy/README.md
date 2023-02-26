# FlatStreamToHierarchy

Dynamic data is a portable class library which brings the power of reactive (rx) to collections.  It is open source and the code base lives here [Dynamic Data on GitHub](https://github.com/RolandPheasant/DynamicData). 

This is an example of how to create a hierachal reactive tree from a flat observable stream.

The demo illustrates how the following code:

```csharp
var loader =  employeeService.Employees.Connect()
    .TransformToTree(employee => employee.BossId)
    .Transform(node => new EmployeeViewModel(node, Promote,Sack))
    .Bind(_employeeViewModels)
    .DisposeMany()
    .Subscribe(); 
``` 

together with some view model and xaml magic produces this
 
![Observable tree example](https://github.com/RolandPheasant/FlatStreamToHierarchy/blob/master/FlatStreamToHierarchy/Screenshot.gif "Observable Tree View")


