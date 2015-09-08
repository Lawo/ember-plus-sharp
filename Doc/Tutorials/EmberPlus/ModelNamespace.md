The Lawo.EmberPlus.Model Namespace    {#TheLawoEmberPlusModelNamespace}
==================================

[TOC]


Prerequisites        {#TheLawoEmberPlusModelNamespace_Prerequisites}
=============

Tiny Ember+        {#TheLawoEmberPlusModelNamespace_Prerequisites_TinyEmberPlus}
-----------

This tutorial will show working code that can be copy-pasted into a console application, which can then be used to
connect to <b>Tiny Ember+</b>. <b>Tiny Ember+</b> is a part of the **Ember+ SDK**, which can be downloaded from the
[Ember+ Project Home](http://code.google.com/p/ember-plus).

Download [sapphire.EmBER](sapphire.EmBER) and open it in <b>Tiny Ember+</b>. The fully expanded tree should look like
this:

![Tiny Ember+ Tree](\ref TinyEmberPlusTree.png)
![Tiny Ember+ Tree](\ref sapphire.EmBER)

\note Please keep <b>Tiny Ember+</b> running throughout this tutorial.


Assemblies        {#TheLawoEmberPlusModelNamespace_Prerequisites_Assemblies}
----------

The code shown in this tutorial requires references to the Lawo and Lawo.EmberPlus assemblies. You can either reference
these assemblies directly or pull the associated projects into your solution and add project references. The projects
can be found under http://lesscm.lawo.de/svn/neo/trunk/Application/Gui.


Introduction        {#TheLawoEmberPlusModelNamespace_Introduction}
============

On an abstract level, an <b>Ember+</b> provider as specified in the
[Ember+ Specification](http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf) can be seen
as a database for elements (an element can either be a parameter, a node, a matrix or a function). When an <b>Ember+</b>
consumer connects to a provider, the consumer queries the provider and builds an (often partial) local copy of the
database. The two databases are then kept in sync through the exchange of appropriate messages until the consumer
decides to terminate the connection.

The types in this namespace aim to automate the above process such that client code can transparently use the local copy
of the database and mostly not care about the following:
- How changes from the provider are applied to the local copy of the database
- How local changes are applied to the provider database
- Whether the consumer is still connected to the provider


Dynamic Interface        {#TheLawoEmberPlusModelNamespace_DynamicInterface}
=================

The dynamic interface can be used to connect to a provider with a database containing unknown elements. It is best
suited for applications that are able to consume a database of just about any contents, like e.g. **Ember+ Viewer**.
As we will see, the dynamic interface is poorly suited for applications that have certain expectations about the contents
of the database.


Create a Local Copy of the Provider Database        {#TheLawoEmberPlusModelNamespace_DynamicInterface_CreateALocalCopy}
--------------------------------------------

### Run Tiny Ember+        {#TheLawoEmberPlusModelNamespace_DynamicInterface_CreateALocalCopy_RunTinyEmberPlus}

See \ref TheLawoEmberPlusModelNamespace_Prerequisites_TinyEmberPlus.


### Create the Project        {#TheLawoEmberPlusModelNamespace_DynamicInterface_CreateALocalCopy_CreateTheProject}

Open **Visual Studio 2013** and create a new **Console Application** project that uses the <b>.NET Framework 4.5</b>.
Add references to Lawo and Lawo.EmberPlus, see \ref TheLawoEmberPlusModelNamespace_Prerequisites_Assemblies.


### Using Declarations        {#TheLawoEmberPlusModelNamespace_DynamicInterface_CreateALocalCopy_UsingDeclarations}

Replace the default using declarations with the following:

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Using Declarations


### TCP Connection and S101        {#TheLawoEmberPlusModelNamespace_DynamicInterface_CreateALocalCopy_TCPConnectionAndS101}

Before we can query the provider we first need to create a connection and then establish the S101 protocol. Since these
first steps will be mostly the same whenever we'd like to connect to a provider, we'll put them into a handy method:

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs S101 Connect Method


### Root Class        {#TheLawoEmberPlusModelNamespace_DynamicInterface_CreateALocalCopy_RootClass}

Next, we need to create a new nested class, an object of which will henceforth represent the root of our local copy of the
provider database.

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Dynamic Root Class

\note The library requires the creation of such a class for the fully dynamic use case although it isn't technically
necessary. We will go into the rationale for this later.


### Main Method        {#TheLawoEmberPlusModelNamespace_DynamicInterface_CreateALocalCopy_MainMethod}

We can now connect to any provider with the following code:

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Main Method

\note The call to [AsyncPump.Run](\ref Lawo.Threading.Tasks.AsyncPump.Run) is only necessary because there is no direct
support for async methods in a **Console Application**. In GUI applications (e.g. **Windows Forms**, **WPF**,
**Windows Store**) the async methods are typically called directly from an async void event handler.


Iterate over the Local Database        {#TheLawoEmberPlusModelNamespace_DynamicInterface_IterateOverTheLocalDatabase}
-------------------------------

Next, we'd like to write out all elements of the local database. This can be achieved with the following method ...

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Write Children

... which we call from \c Main() as follows ...

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Dynamic Iterate

... which produces the following output:

~~~txt
Node Sapphire
  Node Sources
    Node FPGM 1
      Node Fader
        Parameter Number: 1
        Parameter dB Value: -255.999999999998
        Parameter Position: 0
      Node DSP
        Node Input
          Parameter Gain: 0
          Parameter Phase: True
          Parameter LR Mode: 0
        Node Delay
          Parameter Time (ms): 0
      Parameter Audio Type: 2
    Node FPGM 2
      Node Fader
        Parameter Number: 2
        Parameter dB Value: 9
        Parameter Position: 255
      Node DSP
        Node Input
          Parameter Gain: 0
          Parameter Phase: False
          Parameter LR Mode: 0
        Node Delay
          Parameter Time (ms): 0
      Parameter Audio Type: 2
  Node identity
    Parameter product: sapphire
    Parameter company: (c) L-S-B Broadcast Technologies GmbH
~~~


React to Changes        {#TheLawoEmberPlusModelNamespace_DynamicInterface_ReactToChanges}
----------------

### Property Changes        {#TheLawoEmberPlusModelNamespace_DynamicInterface_ReactToChanges_PropertyChanges}

All element interfaces where properties may be changed by the provider ([IElement](\ref Lawo.EmberPlus.Model.IElement),
[IParameter](\ref Lawo.EmberPlus.Model.IParameter), [INode](\ref Lawo.EmberPlus.Model.INode)) derive from
[INotifyPropertyChanged](http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged.aspx). Their
[PropertyChanged](http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged.propertychanged.aspx)
event occurs whenever a provider change has been applied.

The
[PropertyChanged](http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged.propertychanged.aspx)
event also occurs when [IParameter.Value](\ref Lawo.EmberPlus.Model.IParameter.Value) is modified locally.


### Collection Changes        {#TheLawoEmberPlusModelNamespace_DynamicInterface_ReactToChanges_CollectionChanges}

All exposed collections implement the
[INotifyCollectionChanged](http://msdn.microsoft.com/en-us/library/system.collections.specialized.inotifycollectionchanged.aspx)
interface.

\note Throughout its lifetime, a consumer automatically sends *getDirectory* requests to query for children client
code has declared interest in. This is done recursively down to leaf elements and new nodes are only announced through
[INotifyCollectionChanged](http://msdn.microsoft.com/en-us/library/system.collections.specialized.inotifycollectionchanged.aspx)
once all children (including grandchildren, great-grandchildren, etc.) have been received from the provider.


Send Local Changes to the Provider        {#TheLawoEmberPlusModelNamespace_DynamicInterface_SendLocalChanges}
----------------------------------

As you might expect, modifying a value is as easy as setting the
[IParameter.Value](\ref Lawo.EmberPlus.Model.IParameter.Value) property. In a generic application like e.g. a viewer
it would be straightforward to do so. In the code below however, for demonstration purposes we're going to set two
specific parameters. As mentioned earlier, the dynamic interface is not very well suited for tasks that make assumptions
about the contents of the database:

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Dynamic Modify

Specifically, the code above has the following problems:
- No attempt is made to handle exceptions that might result from incorrect assumptions. Such exceptions could be thrown
  when an expected element is not present (e.g. the parameter is named *dBValue* rather than *dB Value*), the actual
  element has a different type (e.g. *Position* is of type [INode](\ref Lawo.EmberPlus.Model.INode) rather than
  [IParameter](\ref Lawo.EmberPlus.Model.IParameter)) or *dB Value* is really a \c long rather than a \c double.
  Robust code would have to handle these exceptions which would make the process even more tedious than it already is.
- The interface offers no way of getting an element by name. The code above has to use **LINQ** to search for the
  desired elements, which is cumbersome and inefficient.

We will see later how the static interface is a much better fit for this scenario.

\note As soon as a parameter is modified locally, provider changes are no longer applied to the parameter until
the \c AutoSendInterval has elapsed or \c SendAsync is called.


Handle Communication Errors        {#TheLawoEmberPlusModelNamespace_DynamicInterface_HandleCommunicationErrors}
---------------------------

Communication errors are signalled through two different mechanisms, see subsections for more information.


### Consumer.ConnectionLost Event        {#TheLawoEmberPlusModelNamespace_DynamicInterface_HandleCommunicationErrors_ConsumerConnectionLostEvent}

At the lowest level, communication errors are signaled through the \c Consumer.ConnectionLost event, which can
be used as demonstrated in the following method:

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Connection Lost

We can test this by simply closing the provider, or by running the provider on a different computer and then
disconnecting the network cable. In the former case
[ConnectionLostEventArgs.Exception](\ref Lawo.EmberPlus.S101.ConnectionLostEventArgs.Exception) is \c null. In the
latter case [ConnectionLostEventArgs.Exception](\ref Lawo.EmberPlus.S101.ConnectionLostEventArgs.Exception) indicates
the reason for the error.


### Exceptions Thrown from Consumer Methods         {#TheLawoEmberPlusModelNamespace_DynamicInterface_HandleCommunicationErrors_ExceptionsThrownFromConsumerMethods}

Both \c Consumer.CreateAsync and \c Consumer.SendAsync can throw communication-related exceptions, which can be
caught and handled as usual.


Static Interface        {#TheLawoEmberPlusModelNamespace_StaticInterface}
================

The static interface should be used in applications that have detailed expectations about the contents of the provider
database. The static interface cannot be used in applications that need to connect to providers with databases of
unknown contents.

The main difference between the dynamic and the static interface is that the former will query the provider for **all**
elements and replicate them in the local database while the latter will only query and replicate the elements that the
client code is interested in. Apart from that the static interface offers a **superset** of the functionality of the
dynamic one.


Iterate over the Local Database        {#TheLawoEmberPlusModelNamespace_StaticInterface_IterateOverTheLocalDatabase}
-------------------------------

It is assumed that you've followed the steps under \ref TheLawoEmberPlusModelNamespace_DynamicInterface, and thus have a
runnable project open in **Visual Studio 2013**.


### Database Classes        {#TheLawoEmberPlusModelNamespace_StaticInterface_IterateOverTheLocalDatabase_DatabaseClasses}

First we need to declare our expectations about the provider database with the following nested types:

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Static Database Types

A few things are of note here:
- The elements *FPGM 1*, *FPGM 2* and *dB Value* have identifiers that cannot be **C#** property names. The associated
  properties need to carry an [ElementAttribute](\ref Lawo.EmberPlus.Model.ElementAttribute) instance with the correct
  identifier.
- The getters and setters of the various properties can have any accessibility. However, since client code should never
  set any of the properties, it is best to declare the setter \c private.
- The \c SuppressMessageAttribute is only present to suppress a **StyleCop** warning. If you do not use **StyleCop**,
  the attribute is unnecessary.
- The constants in the \c enum \c LRMode have names that differ from the ones presented by the provider. This is
  possible because the library only checks that the provider enumeration and the local \c enum have an equal number of
  constants and that they have the same integer values.
- In rare cases, client code might only be interested in the integer value of an enumeration or does not want to
  statically confine the possible values an enumeration can have. In these cases, client code should use
  [IntegerParameter](\ref Lawo.EmberPlus.Model.IntegerParameter) rather than \c EnumParameter.


### Main Method        {#TheLawoEmberPlusModelNamespace_StaticInterface_IterateOverTheLocalDatabase_MainMethod}

We can now iterate over the local database as follows ...

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Static Iterate

... and get the following output:

~~~txt
Node Sapphire
  Node Sources
    Node FPGM 1
      Node Fader
        Parameter dB Value: -255.999999999998
        Parameter Position: 0
      Node DSP
        Node Input
          Parameter Phase: True
          Parameter LR Mode: Stereo
    Node FPGM 2
      Node Fader
        Parameter dB Value: 9
        Parameter Position: 255
      Node DSP
        Node Input
          Parameter Phase: False
          Parameter LR Mode: Stereo
~~~

Notes:
- Compared to the output we've seen under
  \ref TheLawoEmberPlusModelNamespace_DynamicInterface_IterateOverTheLocalDatabase, this time the database only contains
  the elements that we've explicitly declared interest in. So, for large provider databases, the static interface offers
  a way to reduce the memory footprint on the consumer side.
- \c Consumer.CreateAsync verifies that the expectations we've declared match with what the provider sends. Any mismatch
  is signaled with a [ModelException](\ref Lawo.EmberPlus.Model.ModelException). This verification includes the
  presence of required parameters and nodes, the match of types and many other checks.


Send Local Changes to the Provider        {#TheLawoEmberPlusModelNamespace_StaticInterface_SendLocalChanges}
----------------------------------

Now that we have types for everything, changes can be made much more cleanly:

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Static Modify

\note While many simple typos or other programmer mistakes would lead to exceptions with the dynamic interface
(see \ref TheLawoEmberPlusModelNamespace_DynamicInterface_SendLocalChanges), here most of them will be caught by the
compiler. Moreover, since we now have **IntelliSense** for our database, fewer typos are made in the first place. Of
course, whether the expectations expressed in the types match with what a particular provider offers is still verified
at runtime, but it is done in one place (\c Consumer.CreateAsync) and thus client code can handle mismatches with a
single \c catch block.


Unbounded Nodes        {#TheLawoEmberPlusModelNamespace_StaticInterface_UnboundedNodes}
---------------

Suppose the actual number of sources in our provider database was configurable. The consumer would then want to find out
at runtime exactly how many sources there are. This can be achieved with the following classes ...

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Unbounded Database Types

... which are used as follows:

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Collection Node

Note that \c CollectionNode introduces a single point of unboundedness, all the benefits of the static interface and
**IntelliSense** remain available for its children.


Optional Elements        {#TheLawoEmberPlusModelNamespace_StaticInterface_OptionalElements}
-----------------

Imagine that only some of the sources contained a *Fader* node. With the current database types \c Consumer.CreateAsync
would throw an exception for the first source that does not contain a *Fader* node. To avoid this, elements can be
declared optional:

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Optional Fader Source


Nullable Parameters        {#TheLawoEmberPlusModelNamespace_StaticInterface_NullableParameters}
-------------------

The provider is expected to send a value for [BooleanParameter](\ref Lawo.EmberPlus.Model.BooleanParameter),
[IntegerParameter](\ref Lawo.EmberPlus.Model.IntegerParameter),
[OctetstringParameter](\ref Lawo.EmberPlus.Model.OctetstringParameter),
[RealParameter](\ref Lawo.EmberPlus.Model.RealParameter),
[StringParameter](\ref Lawo.EmberPlus.Model.StringParameter) and
\c EnumParameter. Failure to do so lets \c Consumer.CreateAsync fail with an exception. In the vast majority of the
scenarios this behavior is desirable because it relieves client code of checking for \c null values.

In rare case however, a provider may not send an initial value, namely when
[IParameter.Access](\ref Lawo.EmberPlus.Model.IParameter.Access) equals
[ParameterAccess.Write](\ref Lawo.EmberPlus.Model.ParameterAccess) or when
[IParameter.Type](\ref Lawo.EmberPlus.Model.IParameter.Type) equals
[ParameterType.Trigger](\ref Lawo.EmberPlus.Model.ParameterType). For these cases, the library provides nullable
variants for all parameter types, e.g. [NullableBooleanParameter](\ref Lawo.EmberPlus.Model.NullableBooleanParameter).


Mixed Interface        {#TheLawoEmberPlusModelNamespace_MixedInterface}
===============

In some cases we only need to ensure that a few select elements are present in the database, but would still like to
display other elements of the database in a GUI. As we've seen earlier, the dynamic interface does not support this
scenario very nicely because it is cumbersome to navigate to the desired elements. The static interface does not support
this scenario at all.


Iterate over the Local Database        {#TheLawoEmberPlusModelNamespace_MixedInterface_IterateOverTheLocalDatabase}
-------------------------------

It is assumed that you've followed the steps under \ref TheLawoEmberPlusModelNamespace_StaticInterface, and thus have a
runnable project open in **Visual Studio 2013**.


### Database Classes        {#TheLawoEmberPlusModelNamespace_MixedInterface_IterateOverTheLocalDatabase_DatabaseClasses}

First we need to declare our expectations about the provider database with the following nested types:

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Mixed Database Types


### Main Method        {#TheLawoEmberPlusModelNamespace_MixedInterface_IterateOverTheLocalDatabase_MainMethod}

We can now iterate over the local database as follows ...

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Mixed Iterate

... and get the following output:

~~~txt
Node Sapphire
  Node Sources
    Node FPGM 1
      Node Fader
        Parameter dB Value: -255.999999999998
        Parameter Position: 0
      Node DSP
        Node Input
          Parameter Phase: True
          Parameter LR Mode: Stereo
      Parameter Audio Type: 2
    Node FPGM 2
      Node Fader
        Parameter dB Value: 9
        Parameter Position: 255
      Node DSP
        Node Input
          Parameter Phase: False
          Parameter LR Mode: Stereo
      Parameter Audio Type: 2
  Node identity
    Parameter product: sapphire
    Parameter company: (c) L-S-B Broadcast Technologies GmbH
~~~

Note how the *Audio Type* parameters and the *identity* node with all its children now appear although we have not
declared properties for them. This is due to the fact that their parents subclass \c DynamicFieldNode rather than
\c FieldNode.


Send Local Changes to the Provider        {#TheLawoEmberPlusModelNamespace_MixedInterface_SendLocalChanges}
----------------------------------

Since we declared properties for all the elements we'd like to access directly, we can easily make the modifications
we'd like:

\snippet Lawo.EmberPlusTest/Model/TutorialTest.cs Mixed Modify
