''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
' Distributed under the Boost Software License, Version 1.0.
' (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

#Region "Using Declarations"
Imports System
Imports System.Linq
Imports System.Net.Sockets
Imports System.Threading
Imports System.Threading.Tasks
Imports Lawo.EmberPlusSharp.Model
Imports Lawo.EmberPlusSharp.S101
Imports Lawo.Threading.Tasks
'Imports Microsoft.TeamFoundation.Framework.Common
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Xunit.Sdk
#End Region

Public NotInheritable Class TutorialTestVB

    Private Sub New()
    End Sub

    Public Shared Sub DynamicConnectTest()
        Main()
    End Sub

    Public Shared Sub DynamicIterateTest()
#Region "Dynamic Iterate"
        Using cancelToken As New CancellationTokenSource()
            AsyncPump.Run(
            Async Function()
                Using client = Await ConnectAsync("localhost", 9000)
                    Using con = Await Consumer(Of MyRoot).CreateAsync(client)
                        WriteChildren(con.Root, 0)
                    End Using
                End Using
            End Function, cancelToken.Token)
        End Using
#End Region
    End Sub

    Public Shared Sub DynamicModifyTest()
#Region "Dynamic Modify"
        Using cancelToken As New CancellationTokenSource()
            AsyncPump.Run(
            Async Function()
                Using client = Await ConnectAsync("localhost", 9000)
                    Using con = Await Consumer(Of MyRoot).CreateAsync(client)
                        Dim root As INode = con.Root

                        ' Navigate to the parameters we're interested in.
                        Dim sapphire = DirectCast(root.Children.First(Function(c) c.Identifier = "Sapphire"), INode)
                        Dim sources = DirectCast(sapphire.Children.First(Function(c) c.Identifier = "Sources"), INode)
                        Dim fpgm1 = DirectCast(sources.Children.First(Function(c) c.Identifier = "FPGM 1"), INode)
                        Dim fader = DirectCast(fpgm1.Children.First(Function(c) c.Identifier = "Fader"), INode)
                        Dim dbValue = DirectCast(fader.Children.First(Function(c) c.Identifier = "dB Value"), IParameter)
                        Dim position = DirectCast(fader.Children.First(Function(c) c.Identifier = "Position"), IParameter)

                        ' Set parameters to the desired values.
                        dbValue.Value = -67.0
                        position.Value = 128L

                        ' We send the changes back to the provider with the call below. Here, this is necessary so that
                        ' the changes are sent before Dispose is called on the consumer. In a real-world application
                        ' however, SendAsync often does not need to be called explicitly because it is automatically
                        ' called every 100ms as long as there are pending changes. See AutoSendInterval for more
                        ' information.
                        Await con.SendAsync()
                    End Using
                End Using
            End Function, cancelToken.Token)
        End Using

#End Region
    End Sub

    Public Shared Sub ConnectionLostTest()
#Region "Connection Lost"
        Using cancelToken As New CancellationTokenSource()
            AsyncPump.Run(
            Async Function()
                Using client = Await ConnectAsync("localhost", 9000)
                    Using con = Await Consumer(Of MyRoot).CreateAsync(client)
                        Dim connectionLost = New TaskCompletionSource(Of Exception)()
                        AddHandler con.ConnectionLost, Sub(s, e) connectionLost.SetResult(e.Exception)

                        Console.WriteLine("Waiting for the provider to disconnect...")
                        Dim exception = Await connectionLost.Task
                        Console.WriteLine("Connection Lost!")
                        Console.WriteLine("Exception:{0}{1}", exception, Environment.NewLine)
                    End Using
                End Using
            End Function, cancelToken.Token)
        End Using
#End Region
    End Sub

    Public Shared Sub StaticIterateTest()
#Region "Static Iterate"
        Using cancelToken As New CancellationTokenSource()
            AsyncPump.Run(
            Async Function()
                Using client = Await ConnectAsync("localhost", 9000)
                    Using con = Await Consumer(Of SapphireRoot).CreateAsync(client)
                        WriteChildren(con.Root, 0)
                    End Using
                End Using
            End Function, cancelToken.Token)
        End Using
#End Region
    End Sub

    Public Shared Sub StaticReactToChangesTest()
#Region "Static React to Changes"
        Using cancelToken As New CancellationTokenSource()
            AsyncPump.Run(
            Async Function()
                Using client = Await ConnectAsync("localhost", 9000)
                    Using con = Await Consumer(Of SapphireRoot).CreateAsync(client)
                        Dim valueChanged = New TaskCompletionSource(Of String)()
                        Dim positionParameter = con.Root.Sapphire.Sources.Fpgm1.Fader.Position
                        AddHandler positionParameter.PropertyChanged, Sub(s, e) valueChanged.SetResult(DirectCast(s, IElement).GetPath())
                        Console.WriteLine("Waiting for the parameter to change...")
                        Console.WriteLine("A value of the element with the path {0} has been changed.", Await valueChanged.Task)
                    End Using
                End Using
            End Function, cancelToken.Token)
        End Using
#End Region
    End Sub

    Public Shared Sub StaticModifyTest()
#Region "Static Modify"
        Using cancelToken As New CancellationTokenSource()
            AsyncPump.Run(
            Async Function()
                Using client = Await ConnectAsync("localhost", 9000)
                    Using con = Await Consumer(Of SapphireRoot).CreateAsync(client)
                        Dim fader = con.Root.Sapphire.Sources.Fpgm1.Fader
                        fader.DBValue.Value = -67.0
                        fader.Position.Value = 128
                        Await con.SendAsync()
                    End Using
                End Using
            End Function, cancelToken.Token)
        End Using
#End Region
    End Sub

    Public Shared Sub CollectionNodeTest()
#Region "Collection Node"
        Using cancelToken As New CancellationTokenSource()
            AsyncPump.Run(
            Async Function()
                Using client = Await ConnectAsync("localhost", 9000)
                    Using con = Await Consumer(Of UnboundedSapphireRoot).CreateAsync(client)
                        For Each Source In con.Root.Sapphire.Sources.Children
                            Console.WriteLine(Source.Fader.Position.Value)
                        Next
                    End Using
                End Using
            End Function, cancelToken.Token)
        End Using
#End Region
    End Sub

    Public Shared Sub MixedIterateTest()
#Region "Mixed Iterate"
        Using cancelToken As New CancellationTokenSource()
            AsyncPump.Run(
            Async Function()
                Using client = Await ConnectAsync("localhost", 9000)
                    Using con = Await Consumer(Of MixedSapphireRoot).CreateAsync(client)
                        WriteChildren(con.Root, 0)
                    End Using
                End Using
            End Function, cancelToken.Token)
        End Using
#End Region
    End Sub

    Public Shared Sub MixedModifyTest()
#Region "Mixed Modify"
        Using cancelToken As New CancellationTokenSource()
            AsyncPump.Run(
            Async Function()
                Using client = Await ConnectAsync("localhost", 9000)
                    Using con = Await Consumer(Of MixedSapphireRoot).CreateAsync(client)
                        For Each Source In con.Root.Sapphire.Sources.Children
                            Source.Fader.DBValue.Value = -67.0
                            Source.Fader.Position.Value = 128
                            Source.Dsp.Input.LRMode.Value = LRMode.Mono
                            Source.Dsp.Input.Phase.Value = False
                        Next

                        Await con.SendAsync()
                    End Using
                End Using
            End Function, cancelToken.Token)
        End Using
#End Region
    End Sub

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

#Region "Main Method"
    Private Shared Sub Main()
        ' This is necessary so that we can execute async code in a console application.
        Using cancelToken As New CancellationTokenSource()
            AsyncPump.Run(
            Async Function()
                ' Establish S101 protocol
                Using client As S101Client = Await ConnectAsync("localhost", 9000)
                    ' Retrieve *all* elements in the provider database and store them in a local copy
                    Using con As Consumer(Of MyRoot) = Await Consumer(Of MyRoot).CreateAsync(client)
                        ' Get the root of the local database.
                        Dim root As INode = con.Root

                        ' For now just output the number of direct children under the root node.
                        Console.WriteLine(root.Children.Count)
                    End Using
                End Using
            End Function, cancelToken.Token)
        End Using
    End Sub
#End Region

#Region "S101 Connect Method"
    Private Shared Async Function ConnectAsync(host As String, port As Integer) As Task(Of S101Client)
        ' Create TCP connection
        Dim tcpClient = New TcpClient()
        Await tcpClient.ConnectAsync(host, port)

        ' Establish S101 protocol
        ' S101 provides message packaging, CRC integrity checks and a keep-alive mechanism.
        Dim stream = tcpClient.GetStream()
        Return New S101Client(tcpClient, AddressOf stream.ReadAsync, AddressOf stream.WriteAsync)
    End Function
#End Region

#Region "Write Children"
    Private Shared Sub WriteChildren(node As INode, depth As Integer)
        Dim indent = New String(" "c, 2 * depth)

        For Each child In node.Children
            Dim childNode = TryCast(child, INode)

            If childNode IsNot Nothing Then
                Console.WriteLine("{0}Node {1}", indent, child.Identifier)
                WriteChildren(childNode, depth + 1)
            Else
                Dim childParameter = TryCast(child, IParameter)

                If childParameter IsNot Nothing Then
                    Console.WriteLine("{0}Parameter {1}: {2}", indent, child.Identifier, childParameter.Value)
                End If
            End If
        Next
    End Sub
#End Region

#Region "Dynamic Root Class"
    ' Note that the most-derived subtype MyRoot needs to be passed to the generic base class.
    Private NotInheritable Class MyRoot
        Inherits DynamicRoot(Of MyRoot)
    End Class
#End Region

#Region "Static Database Types"
    Private NotInheritable Class SapphireRoot
        Inherits Root(Of SapphireRoot)

        Friend Property Sapphire As Sapphire
    End Class

    Private NotInheritable Class Sapphire
        Inherits FieldNode(Of Sapphire)

        Friend Property Sources As Sources
    End Class

    Private NotInheritable Class Sources
        Inherits FieldNode(Of Sources)

        <Element(Identifier:="FPGM 1")>
        Friend Property Fpgm1 As Source

        <Element(Identifier:="FPGM 2")>
        Friend Property Fpgm2 As Source
    End Class

    Private NotInheritable Class Source
        Inherits FieldNode(Of Source)

        Friend Property Fader As Fader

        <Element(Identifier:="DSP")>
        Friend Property Dsp As Dsp
    End Class

    Private NotInheritable Class Fader
        Inherits FieldNode(Of Fader)

        <Element(Identifier:="dB Value")>
        Friend Property DBValue As RealParameter

        Friend Property Position As IntegerParameter
    End Class

    Private NotInheritable Class Dsp
        Inherits FieldNode(Of Dsp)

        Friend Property Input As Input
    End Class

    Private NotInheritable Class Input
        Inherits FieldNode(Of Input)

        Friend Property Phase() As BooleanParameter

        <Element(Identifier:="LR Mode")>
        Friend Property LRMode As EnumParameter(Of LRMode)
    End Class

    Private Enum LRMode
        Stereo

        RightToBoth

        Side

        LeftToBoth

        Mono

        MidSideToXY
    End Enum
#End Region

#Region "Unbounded Database Types"
    Private NotInheritable Class UnboundedSapphireRoot
        Inherits Root(Of UnboundedSapphireRoot)

        Friend Property Sapphire As UnboundedSapphire
    End Class

    Private NotInheritable Class UnboundedSapphire
        Inherits FieldNode(Of UnboundedSapphire)

        Friend Property Sources As CollectionNode(Of Source)
    End Class
#End Region

#Region "Optional Fader Source"
    Private NotInheritable Class OptionalFaderSource
        Inherits FieldNode(Of OptionalFaderSource)

        <Element(IsOptional:=True)>
        Friend Property Fader As Fader

        <Element(Identifier:="DSP")>
        Friend Property Dsp As Dsp
    End Class
#End Region

#Region "Mixed Database Types"
    ' Subclassing Root means that the Children collection of this node will only contain the elements declared
    ' with properties, in this case a single node with the identifier Sapphire, which is also accessible through
    ' the property.
    Private NotInheritable Class MixedSapphireRoot
        Inherits Root(Of MixedSapphireRoot)

        Friend Property Sapphire As MixedSapphire
    End Class

    ' Subclassing DynamicFieldNode means that the Children collection of this node will contain *all* elements
    ' reported by the provider. Additionally, the node with the identifier Sources is also accessible through the
    ' property.
    Private NotInheritable Class MixedSapphire
        Inherits DynamicFieldNode(Of MixedSapphire)

        Friend Property Sources As CollectionNode(Of MixedSource)
    End Class

    ' Subclassing DynamicFieldNode means that the Children collection of this node will contain *all* elements
    ' reported by the provider. Additionally, the nodes Fader and Dsp are also accessible through their
    ' respective properties. The Fader and Dsp types themselves derive from FieldNode, so their Children
    ' collections will only contain the parameters declared as properties.
    Private NotInheritable Class MixedSource
        Inherits DynamicFieldNode(Of MixedSource)

        Friend Property Fader As Fader

        <Element(Identifier:="DSP")>
        Friend Property Dsp() As Dsp
    End Class
#End Region
End Class
