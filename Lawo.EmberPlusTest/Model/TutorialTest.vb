
'''/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
' <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
'''/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#Region "Using Declarations"
Imports System.Linq
Imports System.Net.Sockets
Imports System.Threading.Tasks
Imports Lawo.EmberPlus.Model
Imports Lawo.EmberPlus.S101
Imports Lawo.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
#End Region

Namespace TutorialVb


    ''' <summary>Tests the <see cref="Consumer{T}"/>-based code shown in the tutorial.</summary>
    <TestClass>
    Public Class TutorialTest
        ''' <summary>Establishes a connection to a provider and creates a dynamic local copy of the database.</summary>
        <TestMethod>
        Public Sub DynamicConnectTest()
            Main()
        End Sub

        ''' <summary>Iterates over the dynamic local database.</summary>
        <TestMethod>
        Public Sub DynamicIterateTest()
            '#Region "Dynamic Iterate"
            AsyncPump.Run(Function()
                              Using client = Await ConnectAsync("localhost", 9000)
				Using consumer = Await Consumer(Of MyRoot).CreateAsync(client)
					WriteChildren(consumer.Root, 0)
                                  End Using
                              End Using

                          End Function)
            '#End Region
        End Sub

        ''' <summary>Modifies parameters in the dynamic local database.</summary>
        <TestMethod>
        Public Sub DynamicModifyTest()
            '#Region "Dynamic Modify"
            AsyncPump.Run(Function()
                              Using client = Await ConnectAsync("localhost", 9000)
				Using consumer = Await Consumer(Of MyRoot).CreateAsync(client)
					Dim root As INode = consumer.Root

                                      ' Navigate to the parameters we're interested in.
                                      Dim sapphire = DirectCast(root.Children.Where(Function(c) c.Identifier = "Sapphire").First(), INode)
                                      Dim sources = DirectCast(sapphire.Children.Where(Function(c) c.Identifier = "Sources").First(), INode)
                                      Dim fpgm1 = DirectCast(sources.Children.Where(Function(c) c.Identifier = "FPGM 1").First(), INode)
                                      Dim fader = DirectCast(fpgm1.Children.Where(Function(c) c.Identifier = "Fader").First(), INode)
                                      Dim dbValue = DirectCast(fader.Children.Where(Function(c) c.Identifier = "dB Value").First(), IParameter)
                                      Dim position = DirectCast(fader.Children.Where(Function(c) c.Identifier = "Position").First(), IParameter)

                                      ' Set parameters to the desired values.
                                      dbValue.Value = -67.0
                                      position.Value = 128L

                                      ' We send the changes back to the provider with the call below. Here, this is necessary so that
                                      ' the changes are sent before Dispose is called on the consumer. In a real-world application
                                      ' however, SendAsync often does not need to be called explicitly because it is automatically
                                      ' called every 100ms as long as there are pending changes. See AutoSendInterval for more
                                      ' information.
                                      Await consumer.SendAsync()
                End Using
                              End Using

                          End Function)
            '#End Region
        End Sub

        ''' <summary>Waits for the connection to be lost.</summary>
        <TestMethod>
        Public Sub ConnectionLostTest()
            '#Region "Connection Lost"
            AsyncPump.Run(Function()
                              Using client = Await ConnectAsync("localhost", 9000)
				Using consumer = Await Consumer(Of MyRoot).CreateAsync(client)
					Dim connectionLost = New TaskCompletionSource(Of Exception)()
                                      consumer.ConnectionLost += Function(s, e) connectionLost.SetResult(e.Exception)

                                      Console.WriteLine("Waiting for the provider to disconnect...")
                                      Dim exception = Await connectionLost.Task
					Console.WriteLine("Connection Lost!")
                                      Console.WriteLine("Exception:{0}{1}", exception, Environment.NewLine)
                                  End Using
                              End Using

                          End Function)
            '#End Region
        End Sub

        ''' <summary>Iterates over the static local database.</summary>
        <TestMethod>
        Public Sub StaticIterateTest()
            '#Region "Static Iterate"
            AsyncPump.Run(Function()
                              Using client = Await ConnectAsync("localhost", 9000)
				Using consumer = Await Consumer(Of SapphireRoot).CreateAsync(client)
					WriteChildren(consumer.Root, 0)
                                  End Using
                              End Using

                          End Function)
            '#End Region
        End Sub

        ''' <summary>Modifies parameters in the dynamic local database.</summary>
        <TestMethod>
        Public Sub StaticModifyTest()
            '#Region "Static Modify"
            AsyncPump.Run(Function()
                              Using client = Await ConnectAsync("localhost", 9000)
				Using consumer = Await Consumer(Of SapphireRoot).CreateAsync(client)
					Dim fader = consumer.Root.Sapphire.Sources.Fpgm1.Fader
                                      fader.DBValue.Value = -67.0
                                      fader.Position.Value = 128
                                      Await consumer.SendAsync()
                End Using
                              End Using

                          End Function)
            '#End Region
        End Sub

        ''' <summary>Tests <see cref="CollectionNode{T}"/>.</summary>
        <TestMethod>
        Public Sub CollectionNodeTest()
            '#Region "Collection Node"
            AsyncPump.Run(Function()
                              Using client = Await ConnectAsync("localhost", 9000)
				Using consumer = Await Consumer(Of UnboundedSapphireRoot).CreateAsync(client)
					For Each source As var In consumer.Root.Sapphire.Sources.Children
                                          Console.WriteLine(source.Fader.Position.Value)
                                      Next
                                  End Using
                              End Using

                          End Function)
            '#End Region
        End Sub

        ''' <summary>Iterates over the mixed local database.</summary>
        <TestMethod>
        Public Sub MixedIterateTest()
            '#Region "Mixed Iterate"
            AsyncPump.Run(Function()
                              Using client = Await ConnectAsync("localhost", 9000)
				Using consumer = Await Consumer(Of MixedSapphireRoot).CreateAsync(client)
					WriteChildren(consumer.Root, 0)
                                  End Using
                              End Using

                          End Function)
            '#End Region
        End Sub

        ''' <summary>Modifies parameters in the mixed local database.</summary>
        <TestMethod>
        Public Sub MixedModifyTest()
            '#Region "Mixed Modify"
            AsyncPump.Run(Function()
                              Using client = Await ConnectAsync("localhost", 9000)
				Using consumer = Await Consumer(Of MixedSapphireRoot).CreateAsync(client)
					For Each source As var In consumer.Root.Sapphire.Sources.Children
                                          source.Fader.DBValue.Value = -67.0
                                          source.Fader.Position.Value = 128
                                          source.Dsp.Input.LRMode.Value = LRMode.Mono
                                          source.Dsp.Input.Phase.Value = False
                                      Next

                                      Await consumer.SendAsync()
                End Using
                              End Using

                          End Function)
            '#End Region
        End Sub

        '''/////////////////////////////////////////////////////////////////////////////////////////////////////////////

#Region "Main Method"
        Private Shared Sub Main()
            ' This is necessary so that we can execute async code in a console application.
            AsyncPump.Run(Function()
                              ' Establish S101 protocol
                              Using client As S101Client = Await ConnectAsync("localhost", 9000)

				' Query the provider database for *all* elements and store them in a local copy
				Using consumer As Consumer(Of MyRoot) = Await Consumer(Of MyRoot).CreateAsync(client)
					' Get the root of the local database.
					Dim root As INode = consumer.Root

                                      ' For now just output the number of direct children under the root node.
                                      Console.WriteLine(root.Children.Count)
                                  End Using
                              End Using

                          End Function)
        End Sub
#End Region

#Region "S101 Connect Method"
        Private Shared Function ConnectAsync(host As String, port As Integer) As Task(Of S101Client)
            ' Create TCP connection
            Dim tcpClient = New TcpClient()
            Await tcpClient.ConnectAsync(host, port)

            ' Establish S101 protocol
            ' S101 provides message packaging, CRC integrity checks and a keep-alive mechanism.
            Dim stream = tcpClient.GetStream()
            Return New S101Client(tcpClient, stream.ReadAsync, stream.WriteAsync)
        End Function
#End Region

#Region "Write Children"
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification:="Test code.")>
        Private Shared Sub WriteChildren(node As INode, depth As Integer)
            Dim indent = New String(" "c, 2 * depth)

            For Each child As var In node.Children
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
            Friend Property Sapphire() As Sapphire
                Get
                    Return m_Sapphire
                End Get
                Private Set
                    m_Sapphire = Value
                End Set
            End Property
            Private m_Sapphire As Sapphire
        End Class

        Private NotInheritable Class Sapphire
            Inherits FieldNode(Of Sapphire)
            Friend Property Sources() As Sources
                Get
                    Return m_Sources
                End Get
                Private Set
                    m_Sources = Value
                End Set
            End Property
            Private m_Sources As Sources
        End Class

        Private NotInheritable Class Sources
            Inherits FieldNode(Of Sources)
            <Element(Identifier:="FPGM 1")>
            Friend Property Fpgm1() As Source
                Get
                    Return m_Fpgm1
                End Get
                Private Set
                    m_Fpgm1 = Value
                End Set
            End Property
            Private m_Fpgm1 As Source

            <Element(Identifier:="FPGM 2")>
            Friend Property Fpgm2() As Source
                Get
                    Return m_Fpgm2
                End Get
                Private Set
                    m_Fpgm2 = Value
                End Set
            End Property
            Private m_Fpgm2 As Source
        End Class

        Private NotInheritable Class Source
            Inherits FieldNode(Of Source)
            Friend Property Fader() As Fader
                Get
                    Return m_Fader
                End Get
                Private Set
                    m_Fader = Value
                End Set
            End Property
            Private m_Fader As Fader

            <Element(Identifier:="DSP")>
            Friend Property Dsp() As Dsp
                Get
                    Return m_Dsp
                End Get
                Private Set
                    m_Dsp = Value
                End Set
            End Property
            Private m_Dsp As Dsp
        End Class

        Private NotInheritable Class Fader
            Inherits FieldNode(Of Fader)
            <Element(Identifier:="dB Value")>
            Friend Property DBValue() As RealParameter
                Get
                    Return m_DBValue
                End Get
                Private Set
                    m_DBValue = Value
                End Set
            End Property
            Private m_DBValue As RealParameter

            Friend Property Position() As IntegerParameter
                Get
                    Return m_Position
                End Get
                Private Set
                    m_Position = Value
                End Set
            End Property
            Private m_Position As IntegerParameter
        End Class

        Private NotInheritable Class Dsp
            Inherits FieldNode(Of Dsp)
            Friend Property Input() As Input
                Get
                    Return m_Input
                End Get
                Private Set
                    m_Input = Value
                End Set
            End Property
            Private m_Input As Input
        End Class

        Private NotInheritable Class Input
            Inherits FieldNode(Of Input)
            Friend Property Phase() As BooleanParameter
                Get
                    Return m_Phase
                End Get
                Private Set
                    m_Phase = Value
                End Set
            End Property
            Private m_Phase As BooleanParameter

            <Element(Identifier:="LR Mode")>
            Friend Property LRMode() As EnumParameter(Of LRMode)
                Get
                    Return m_LRMode
                End Get
                Private Set
                    m_LRMode = Value
                End Set
            End Property
            Private m_LRMode As EnumParameter(Of LRMode)
        End Class

        <System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification:="Demo Code.")>
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
            Friend Property Sapphire() As UnboundedSapphire
                Get
                    Return m_Sapphire
                End Get
                Private Set
                    m_Sapphire = Value
                End Set
            End Property
            Private m_Sapphire As UnboundedSapphire
        End Class

        Private NotInheritable Class UnboundedSapphire
            Inherits FieldNode(Of UnboundedSapphire)
            Friend Property Sources() As CollectionNode(Of Source)
                Get
                    Return m_Sources
                End Get
                Private Set
                    m_Sources = Value
                End Set
            End Property
            Private m_Sources As CollectionNode(Of Source)
        End Class
#End Region

#Region "Optional Fader Source"
        Private NotInheritable Class OptionalFaderSource
            Inherits FieldNode(Of OptionalFaderSource)
            <Element(IsOptional:=True)>
            Friend Property Fader() As Fader
                Get
                    Return m_Fader
                End Get
                Private Set
                    m_Fader = Value
                End Set
            End Property
            Private m_Fader As Fader

            <Element(Identifier:="DSP")>
            Friend Property Dsp() As Dsp
                Get
                    Return m_Dsp
                End Get
                Private Set
                    m_Dsp = Value
                End Set
            End Property
            Private m_Dsp As Dsp
        End Class
#End Region

#Region "Mixed Database Types"
        ' Subclassing Root means that the Children collection of this node will only contain the elements declared
        ' with properties, in this case a single node with the identifier Sapphire, which is also accessible through
        ' the property.
        Private NotInheritable Class MixedSapphireRoot
            Inherits Root(Of MixedSapphireRoot)
            Friend Property Sapphire() As MixedSapphire
                Get
                    Return m_Sapphire
                End Get
                Private Set
                    m_Sapphire = Value
                End Set
            End Property
            Private m_Sapphire As MixedSapphire
        End Class

        ' Subclassing DynamicFieldNode means that the Children collection of this node will contain *all* elements
        ' reported by the provider. Additionally, the node with the identifier Sources is also accessible through the
        ' property.
        Private NotInheritable Class MixedSapphire
            Inherits DynamicFieldNode(Of MixedSapphire)
            Friend Property Sources() As CollectionNode(Of MixedSource)
                Get
                    Return m_Sources
                End Get
                Private Set
                    m_Sources = Value
                End Set
            End Property
            Private m_Sources As CollectionNode(Of MixedSource)
        End Class

        ' Subclassing DynamicFieldNode means that the Children collection of this node will contain *all* elements
        ' reported by the provider. Additionally, the nodes Fader and Dsp are also accessible through their
        ' respective properties. The Fader and Dsp types themselves derive from FieldNode, so their Children
        ' collections will only contain the parameters declared as properties.
        Private NotInheritable Class MixedSource
            Inherits DynamicFieldNode(Of MixedSource)
            Friend Property Fader() As Fader
                Get
                    Return m_Fader
                End Get
                Private Set
                    m_Fader = Value
                End Set
            End Property
            Private m_Fader As Fader

            <Element(Identifier:="DSP")>
            Friend Property Dsp() As Dsp
                Get
                    Return m_Dsp
                End Get
                Private Set
                    m_Dsp = Value
                End Set
            End Property
            Private m_Dsp As Dsp
        End Class
#End Region
    End Class
End Namespace

'=======================================================
'Service provided by Telerik (www.telerik.com)
'Conversion powered by NRefactory.
'Twitter: @telerik
'Facebook: facebook.com/telerik
'=======================================================
