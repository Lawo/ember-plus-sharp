MVVM Tutorial     {#MvvmTutorial}
=============

[TOC]


Overview        {#MvvmTutorial_Overview}
========

For a nice introduction to MVVM please refer to
[Ivo Manolov's Blog](http://blogs.msdn.com/b/ivo_manolov/archive/2012/03/17/10284665.aspx).

\note Please ignore the section discussing the
[ICommand](http://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.input.icommand.aspx) interface and its
implementation. We will see below how
[ICommand](http://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.input.icommand.aspx) is no longer
necessary with XAML frameworks that offer triggers or behaviors.

To summarize, the important facts of MVVM are the following:
- At compile time, there is a unidirectional dependency between the different entities: The View references the
  ViewModel, which in turn references the Model. However, the ViewModel does not know anything about the View and the
  Model does not know anything about the View or the ViewModel.
- The main responsibility of the ViewModel is to provide the data in the format needed by the View, offer methods for
  the actions that can be performed on the View and provide events that the View needs to subscribe to.
- The ViewModel is fully GUI-technology-agnostic and can be exercised by automated tests.
- The View defines how the data of the ViewModel is presented to the user. It is often implemented solely in XAML.
  In the cases where C# code is necessary, the code only concerns itself with the presentation of the data but not the
  business logic.
- The Model is implemented without consideration for how the GUI will look like. It simply provides all necessary data,
  offers methods to modify the data and enforces the business logic. The data is provided in the format that is most
  convenient and efficient for the Model.

A ViewModel is particularly easy to consume from a XAML-based GUI (WPF, Silverlight, Windows Store App, etc.):
- Databinding: Normal properties as well as
  [ObservableCollection<T>](http://msdn.microsoft.com/en-us/library/ms668604.aspx) properties can be
  bound to controls directly in XAML (no code behind is necessary). Changes in the GUI are automatically pushed to the
  bound properties and changes to the properties automatically lead to updates of the GUI.
- Control Behavior:
  [Control.IsEnabled](http://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.controls.control.isenabled.aspx)
  and similar GUI behavior properties can be bound to properties of the ViewModel directly in XAML.
- Triggers and Behaviors: Events on GUI controls can be bound to ViewModel methods directly in XAML, such
  that a method is called whenever the associated event is raised (see
  [CallMethodAction](http://msdn.microsoft.com/en-us/library/windows/apps/xaml/microsoft.xaml.interactions.core.callmethodaction.aspx)).

\note For simple applications, it is often unclear at first where to best draw the line between the Model and the
ViewModel. If one follows the MVVM pattern to the letter and implements all the business logic in the Model, the
ViewModel often degenerates into a class that does little else than forward function calls between the Model and the
View. In such cases I think it is perfectly acceptable to implement much or even all business logic in the ViewModel.
Note that the most important aspect of MVVM is the separation between View and ViewModel. Whether and where to separate
the Model from the ViewModel is of lower importance and should be decided based on the use cases. For example, the
Model in the **GlowAnalyzerProxy** application is the `Settings` class, which is responsible for the storage of the
application settings. All other business logic is implemented in the `MainWindowViewModel`.


Data Synchronization        {#MvvmTutorial_DataSynchronization}
====================

The ViewModel and the View each hold their own copy of the data shown on the View. Moreover, the ViewModel often also
needs to duplicate at least some of the data provided by the Model. Data therefore needs to be synchronized between
these entities. The following sections detail the responsibilities of the developer and suggest supporting classes.

\note The developer responsibilities below should be read as suggestions rather than rules. For complex scenarios, it
probably makes sense to implement things as suggested. In simple cases shortcuts can be taken, for example:
- For a GUI that only displays data that never changes, it makes little sense to implement change propagation. It is
  sufficient to implement initialization.
- For data that is only ever displayed and changed from a single view, change propagation from Model to ViewModel to
  View is unnecessary and does not need to be implemented.


Initialization        {#MvvmTutorial_DataSynchronization_Initialization}
--------------

| Entity    | Developer Responsibilities                                                                                                                                 | Supporting Classes                                                                                                                                                      |
|-----------|------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Model     | Implement property getters                                                                                                                                 |                                                                                                                                                                         |
| ViewModel | Call property getters in Model, implement property getters                                                                                                 | [OneWayBinding](\ref Lawo.ComponentModel.OneWayBinding), [TwoWayBinding](\ref Lawo.ComponentModel.TwoWayBinding), [MultiBinding](\ref Lawo.ComponentModel.MultiBinding) |
| View      | Set [DataContext](http://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.frameworkelement.datacontext.aspx), use binding expressions in XAML |                                                                                                                                                                         |


Change Propagation from View to ViewModel to Model        {#MvvmTutorial_DataSynchronization_ChangePropagationFromViewToModel}
--------------------------------------------------

| Entity    | Developer Responsibilities                                                                                                                                 | Supporting Classes                                                                                                                                                      |
|-----------|------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Model     | Provide property setters                                                                                                                                   |                                                                                                                                                                         |
| ViewModel | Provide property setters, call property setters in Model                                                                                                   | [OneWayBinding](\ref Lawo.ComponentModel.OneWayBinding), [TwoWayBinding](\ref Lawo.ComponentModel.TwoWayBinding), [MultiBinding](\ref Lawo.ComponentModel.MultiBinding) |
| View      | Set [DataContext](http://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.frameworkelement.datacontext.aspx), use binding expressions in XAML |                                                                                                                                                                         |


Change Propagation from Model to ViewModel to View        {#MvvmTutorial_DataSynchronization_ChangePropagationFromModelToView}
--------------------------------------------------

| Entity    | Developer Responsibilities                                                                                                                                                      | Supporting Classes                                                                                                                                                                                                                               |
|-----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Model     | Implement [INotifyPropertyChanged](http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged.aspx)                                                   | [NotifyPropertyChanged](\ref Lawo.ComponentModel.NotifyPropertyChanged)                                                                                                                                                                          |
| ViewModel | Implement [INotifyPropertyChanged](http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged.aspx), subscribe to PropertyChanged events of the Model | [NotifyPropertyChanged](\ref Lawo.ComponentModel.NotifyPropertyChanged), [OneWayBinding](\ref Lawo.ComponentModel.OneWayBinding), [TwoWayBinding](\ref Lawo.ComponentModel.TwoWayBinding), [MultiBinding](\ref Lawo.ComponentModel.MultiBinding) |
| View      | Set [DataContext](http://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.frameworkelement.datacontext.aspx), use binding expressions in XAML                      |                                                                                                                                                                                                                                                  |


Example        {#MvvmTutorial_Example}
=======

The sections below discuss the application of MVVM with the help of actual code in the **GlowAnalyzerProxy** application.

![GlowAnalyzerProxy Application Screenshot](\ref GlowAnalyzerProxy.png)


Model        {#MvvmTutorial_Model}
-----

~~~cs
internal sealed class Settings : global::System.Configuration.ApplicationSettingsBase
{
    public static Settings Default { get { ... } }

    public string ProviderHostName { get { ... } set { ... } }

    public string ProviderPort { get { ... } set { ... } }

    public string ListeningPort { get { ... } set { ... } }
        
    public string LogFolder { get { ... } set { ... } }
}
~~~

The Model is not particularly interesting, because its only purpose is to handle settings persistence. Note that this is
not a particularly typical or even exemplary MVVM Model, see the note in the [Overview](\ref MvvmTutorial_Overview).


### Change Notification ###        {#MvvmTutorial_Model_ChangeNotification}

Often the ViewModel duplicates at least some information provided by the Model, which is why the ViewModel needs to be
notified when the data in the Model changes.
[ApplicationSettingsBase](http://msdn.microsoft.com/en-us/library/system.configuration.applicationsettingsbase.aspx)
already implements
[INotifyPropertyChanged](http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged.aspx), so
the code generated by the application settings designer works just fine for us. A more typical Model would simply derive
from [NotifyPropertyChanged](\ref Lawo.ComponentModel.NotifyPropertyChanged) and implement very similar change
notification as a ViewModel.


ViewModel        {#MvvmTutorial_Examples_ViewModel}
---------

~~~cs
internal sealed class MainWindowViewModel : NotifyPropertyChanged
{
    public string Title { get { ... } }

    public string ListeningPort { get { ... } set { ... } }

    public string ProviderHostName { get { ... } set { ... } }

    public string ProviderPort { get { ... } set { ... } }

    public string LogFolder { get { ... } set { ... } }

    public bool CanEditSettings { get { ... } }

    public bool CanStart { get { ... } }

    public void Start() { ... }

    public bool CanStop { get { ... } }

    public void Stop() { ... }

    public void SaveSettings() { ... }

    public ConnectionViewModel ConsumerConnection { get { ... } }

    public ConnectionViewModel ProviderConnection { get { ... } }

    public ReadOnlyObservableCollection<Event> Events { get { ... } }

    public LogEntry SelectedEvent { get { ... } set { ... } }

    public FlowDocument SelectedEventDetail { get { ... } set { ... } }

    internal MainWindowViewModel(Settings settings) { ... }
}
~~~

~~~cs
    internal sealed class ConnectionViewModel : NotifyPropertyChanged
    {
        public string ConnectionCount { get { ... } }

        public string BytesReceived { get { ... } }

        public string SecondsSinceLastReceived { get { ... } }
    }
~~~

A few things are of note here, please see the sections below for more information.


### Members ###        {#MvvmTutorial_Examples_ViewModel_Members}

Please compare the ViewModel code with the screenshot of the application above. Note how there is roughly a 1:1 mapping
between the controls on the GUI and the members of the ViewModel. There are different types of members:
1. Properties, the values of which are directly shown and sometimes also edited on the GUI: `Title`,  `ListeningPort`,
   `ProviderHostName`, `ProviderPort`, `LogFolder`, `Events`, `SelectedEvent`, `SelectedEventDetail`.
2. Properties, the values of which contain other properties, which in turn are shown on the GUI :
   `ConsumerConnection` and `ProviderConnection`.
3. Properties, the values of which are used to define how the controls behave:
   - `CanEditSettings`: Defines the value of the
      [IsEnabled](http://msdn.microsoft.com/en-us/library/system.windows.uielement.isenabled.aspx) property
      of the [TextBox](http://msdn.microsoft.com/en-us/library/system.windows.controls.textbox.aspx) controls for
      **Listening Port**, **Provider Host Name**, **Provider Port** and the \b ...
      [Button](http://msdn.microsoft.com/en-us/library/system.windows.controls.button.aspx).
   - `CanStart`: Defines the value of the
      [IsEnabled](http://msdn.microsoft.com/en-us/library/system.windows.uielement.isenabled.aspx) property of the
      **Start** [Button](http://msdn.microsoft.com/en-us/library/system.windows.controls.button.aspx).
   - `CanStop`: Defines the value of the
      [IsEnabled](http://msdn.microsoft.com/en-us/library/system.windows.uielement.isenabled.aspx) property of the
      **Stop** [Button](http://msdn.microsoft.com/en-us/library/system.windows.controls.button.aspx).
4. Methods, which are called when the user makes an input:
   - `Start()`: Is called when the **Start** button is clicked.
   - `Stop()`: Is called when the **Stop** button is clicked.
   - `SaveSettings()`: Is called when the user clicks the **x** in the top right corner of the application.

\note We are deliberately not using an
[ICommand](http://msdn.microsoft.com/en-us/library/windows/apps/xaml/system.windows.input.icommand.aspx) implementation
(usually called DelegateCommand or RelayCommand) to combine e.g. `CanStart` and `Start()`. Although doing so would make
it slightly easier to bind e.g. a button to a command, providing said command is harder than implementing two methods.
For actions that are always available (like e.g. `SaveSettings()` above) we only need to implement one method and by
doing so we efficiently communicate that this action is always available.


### Change Notification ###        {#MvvmTutorial_Examples_ViewModel_ChangeNotification}

The View holds a copy of the data provided by the ViewModel, which is why the View needs to be notified when the data in
the ViewModel changes. All XAML-based Views automatically look for implementations of
[INotifyPropertyChanged](http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged.aspx) and
[INotifyCollectionChanged](http://msdn.microsoft.com/en-us/library/system.collections.specialized.inotifycollectionchanged.aspx)
and subscribe to the provided events as appropriate. ViewModel developers can reuse the following implementations:

- [NotifyPropertyChanged](\ref Lawo.ComponentModel.NotifyPropertyChanged): ViewModel implementations as well as the
  types of composite properties usually derive from this base class.
- Collection properties are usually of the type
  [ObservableCollection<T>](http://msdn.microsoft.com/en-us/library/ms668604.aspx) or
  [ReadOnlyObservableCollection<T>](http://msdn.microsoft.com/en-us/library/ms668620.aspx).


#### Primitive Properties ####        {#MvvmTutorial_Examples_ViewModel_ChangeNotification_PrimitiveProperties}

A primitive property provides data that can directly be shown in the View (e.g. `string`, `double`, `int`, etc.). A
property where the value may change while it is being displayed in the View typically looks as follows:

\snippet Lawo.GlowAnalyzerProxy.Main/MainWindowViewModel.cs ReadWriteProperty

\note [NotifyPropertyChanged.SetValue()](\ref Lawo.ComponentModel.NotifyPropertyChanged) does its magic with the
[CallerMemberNameAttribute](http://msdn.microsoft.com/en-us/library/system.runtime.compilerservices.callermembernameattribute.aspx).
[NotifyPropertyChanged.SetValue()](\ref Lawo.ComponentModel.NotifyPropertyChanged) should therefore only ever be called
directly from a setter of a public property.

Of course, properties that never change their value do not need to concern themselves with change notification:

\snippet Lawo.GlowAnalyzerProxy.Main/MainWindowViewModel.cs ReadOnlyProperty


#### Composite Properties ####        {#MvvmTutorial_Example_ViewModel_ChangeNotification_CompositeProperties}

The getter of a composite property returns a value, which cannot directly be shown on the GUI:

\snippet Lawo.GlowAnalyzerProxy.Main/MainWindowViewModel.cs CompositeProperty

In this case the property value never changes. Of course, in general such properties can change their value too. Then,
change notification needs to be implemented by calling `SetValue` in the property setter just like a primitive
property setter does.

Since the View will bind to properties of the returned value, the underlying type must also implement
[INotifyPropertyChanged](http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged.aspx),
here again by deriving from [NotifyPropertyChanged](\ref Lawo.ComponentModel.NotifyPropertyChanged).


#### Collection Properties ####        {#MvvmTutorial_Example_ViewModel_ChangeNotification_CollectionProperties}

Whenever a View needs to display multiple items in a list, the associated ViewModel typically offers the necessary
data through a property getter that returns a collection implementing the
[INotifyCollectionChanged](http://msdn.microsoft.com/en-us/library/system.collections.specialized.inotifycollectionchanged.aspx)
interface:

\snippet Lawo.GlowAnalyzerProxy.Main/MainWindowViewModel.cs CollectionProperty

The .NET framework implementations
[ObservableCollection<T>](http://msdn.microsoft.com/en-us/library/ms668604.aspx) and
[ReadOnlyObservableCollection<T>](http://msdn.microsoft.com/en-us/library/ms668620.aspx) are almost always sufficient.
The former should only be used if the View itself can directly add and/or remove items. The latter is preferable when
such operations are offered through ViewModel methods and of course also when the collection is immutable from the View.


### Binding to Model Data ###        {#MvvmTutorial_Example_ViewModel_BindingToModelData}

Without library support, binding the value of a C# source property to the value of C# target property is rather tedious
and error-prone. This is due to the fact that implementations of
[INotifyPropertyChanged](http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged.aspx)
signal a property change by calling the subscribed handler with the property name as a string. The handler then
needs to compare the passed string to find out exactly which property has been changed. The
[Lawo.Reflection](\ref Lawo.Reflection) and [Lawo.ComponentModel](\ref Lawo.ComponentModel)
namespaces offer a few tools to make this process much easier.

Bindings are typically created in the ViewModel constructor.

\note All the binding methods discussed below return an object that represents the newly created binding. The binding
can be broken by calling [Dispose()](http://msdn.microsoft.com/en-us/library/system.idisposable.dispose.aspx). In many
cases however, it is sensible to never explicitly remove a binding.


#### Two-Way Binding ####        {#MvvmTutorial_Example_ViewModel_BindingToModelData_TwoWayBinding}

\snippet Lawo.GlowAnalyzerProxy.Main/MainWindowViewModel.cs TwoWayBinding

[TwoWayBinding](\ref Lawo.ComponentModel.TwoWayBinding) can be used to simply "forward" a property from the Model to the
ViewModel and vice versa. Overloads that accept conversion functions can be used to convert between properties of
differing types.


#### One-Way Binding ####        {#MvvmTutorial_Example_ViewModel_BindingToModelData_OneWayBinding}

[OneWayBinding](\ref Lawo.ComponentModel.OneWayBinding) can be used if changes only need to be propagated from the Model
to the ViewModel but not the other way round. ViewModel properties bound in such a way are typically read-only for the
View.


#### Multi-Binding ####        {#MvvmTutorial_Example_ViewModel_BindingToModelData_MultiBinding}

A [MultiBinding](\ref Lawo.ComponentModel.MultiBinding) can be used if the value of a ViewModel property depends on
multiple other properties.


#### Calculated Property ####        {#MvvmTutorial_Example_ViewModel_BindingToModelData_CalculatedProperty}

A [CalculatedProperty](\ref Lawo.ComponentModel.CalculatedProperty) is a slightly easier way than
\ref MvvmTutorial_Example_ViewModel_BindingToModelData_MultiBinding to implement a ViewModel property that depends on
multiple other properties. The differences are:

- A [MultiBinding](\ref Lawo.ComponentModel.MultiBinding) can be created between any source and target properties as
  long as all owners of source properties implement
  [INotifyPropertyChanged](http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged.aspx).
  A [CalculatedProperty](\ref Lawo.ComponentModel.CalculatedProperty) additionally requires that the owner of the target
  property derives from [NotifyPropertyChanged](\ref Lawo.ComponentModel.NotifyPropertyChanged).
- A [MultiBinding](\ref Lawo.ComponentModel.MultiBinding) requires a target property with a getter and a setter. With
  [CalculatedProperty](\ref Lawo.ComponentModel.CalculatedProperty) only the getter needs to implemented.

To implement a calculated property in `MainWindowViewModel`, firstly we need a field:

\snippet Lawo.GlowAnalyzerProxy.Main/MainWindowViewModel.cs CalculatedProperty1

Secondly, the field needs to be assigned an appropriately initialized instance:

\snippet Lawo.GlowAnalyzerProxy.Main/MainWindowViewModel.cs CalculatedProperty2

The first 4 arguments represent the source properties that the calculated property is derived from. The next argument
calculates the value of the target property from the values of the source properties. The last argument represents the
target property.

The implementation of the actual property looks as follows:

\snippet Lawo.GlowAnalyzerProxy.Main/MainWindowViewModel.cs CalculatedProperty3


View        {#MvvmTutorial_Example_View}
----

### Binding to ViewModel Data ###        {#MvvmTutorial_Example_View_BindingToViewModelData}

Before the ViewModel data can be accessed by the View, an object of the former needs to be set as the
[DataContext](http://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.frameworkelement.datacontext.aspx) of
the latter. Here, this is done in the code-behind of the View:

\snippet Lawo.GlowAnalyzerProxy.Main/MainWindow.xaml.cs SetDataContext

Note that an object of the `Settings` class (which acts as the Model here) is passed to the `MainWindowViewModel`
constructor.


#### Primitive Properties ####        {#MvvmTutorial_Example_View_BindingToViewModelData_PrimitiveProperties}

\snippet Lawo.GlowAnalyzerProxy.Main/MainWindow.xaml PrimitivePropertyBinding

The `Text="{Binding ProviderHostName}"` part in the code above establishes a two-way binding between
`MainWindowViewModel.ProviderHostName` and
[TextBox.Text](http://msdn.microsoft.com/en-us/library/system.windows.controls.textbox.text.aspx). This means that the
following operations take place automatically:
- Initialization: When the 
  [DataContext](http://msdn.microsoft.com/en-us/library/system.windows.frameworkelement.datacontext.aspx) is set,
  `MainWindow` automatically calls the `MainWindowViewModel.ProviderHostName` getter and assigns the value to
  [TextBox.Text](http://msdn.microsoft.com/en-us/library/system.windows.controls.textbox.text.aspx). `MainWindow` also
  automatically subscribes to
  [NotifyPropertyChanged.PropertyChanged](\ref Lawo.ComponentModel.NotifyPropertyChanged.PropertyChanged).
- Change Propagation from View to ViewModel: When the user changes the text in the
  [TextBox](http://msdn.microsoft.com/en-us/library/system.windows.controls.textbox.aspx), `MainWindow` automatically
  sets the new value on `MainWindowViewModel.ProviderHostName`.
- Change Propagation from ViewModel to View: When the business logic changes `MainWindowViewModel.ProviderHostName`,
  the [NotifyPropertyChanged.PropertyChanged](\ref Lawo.ComponentModel.NotifyPropertyChanged.PropertyChanged) event is
  raised. Since `MainWindow` is subscribed to the event it then automatically calls the
  `MainWindowViewModel.ProviderHostName` property getter and sets the value on
  [TextBox.Text](http://msdn.microsoft.com/en-us/library/system.windows.controls.textbox.text.aspx).


#### Composite Properties ####        {#MvvmTutorial_Example_View_BindingToViewModelData_CompositeProperties}

\snippet Lawo.GlowAnalyzerProxy.Main/MainWindow.xaml CompositePropertyBinding

The `DataContext="{Binding ConsumerConnection}"` and `DataContext="{Binding ProviderConnection}"` parts in the code
above bind the `ConnectionViewModel` values returned by the properties to the
[DataContext](http://msdn.microsoft.com/en-us/library/system.windows.frameworkelement.datacontext.aspx) of the two
`ConnectionStatusUserControl` instances. This allows `ConnectionStatusUserControl` to bind directly to
`ConnectionViewModel` properties:

\snippet Lawo.GlowAnalyzerProxy.Main/ConnectionStatusUserControl.xaml PrimitivePropertyBinding

Note how we have used `ConnectionStatusUserControl` in `MainWindow` and `ConnectionViewModel` in `MainWindowViewModel`
to avoid duplicating identical parts.


#### Collection Properties ####        {#MvvmTutorial_Example_View_BindingToViewModelData_CollectionProperties}

\snippet Lawo.GlowAnalyzerProxy.Main/MainWindow.xaml CollectionPropertyBinding

The `ItemsSource="{Binding Events}"` part ensures that the elements in the
[ReadOnlyObservableCollection<T>](http://msdn.microsoft.com/en-us/library/ms668620.aspx) are shown in the
[DataGrid](http://msdn.microsoft.com/en-us/library/system.windows.controls.datagrid.aspx). Since the collection
implements
[INotifyCollectionChanged](http://msdn.microsoft.com/en-us/library/system.collections.specialized.inotifycollectionchanged.aspx),
any changes to the collection are immediately reflected in the GUI. The `SelectedItem="{Binding SelectedEvent}"` part
makes the currently selected `Event` available in the `MainWindowViewModel`.


### Calling ViewModel Methods ###        {#MvvmTutorial_Example_View_CallingViewModelMethods}

\snippet Lawo.GlowAnalyzerProxy.Main/MainWindow.xaml CallViewModelMethod

The above is WPF specific, similar mechanisms exist for Windows Store Apps, see
[CallMethodAction](http://msdn.microsoft.com/en-us/library/windows/apps/xaml/microsoft.xaml.interactions.core.callmethodaction.aspx)).