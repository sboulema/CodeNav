Imports System

Namespace CodeNav.SmokeTest

    Public Interface IWorker
        Sub Execute(value As Integer)
        Property Name As String
        Event Completed As EventHandler
    End Interface

    Public MustInherit Class WorkerBase
        Public MustOverride Sub Run()
    End Class

    Public Class SampleWorker
        Inherits WorkerBase
        Implements IWorker

        Private _first, _second As Integer

        Public Const Maximum As Integer = 10

        Public Sub New(value As Integer)
            _first = value
        End Sub

        Public Property Name As String Implements IWorker.Name

        Public Event Completed As EventHandler Implements IWorker.Completed

        Public Overrides Sub Run()
            Execute(_first)
        End Sub

#Region "Worker members"
#Region "Execution"
        Public Sub Execute(value As Integer) Implements IWorker.Execute
            _second = value
            RaiseEvent Completed(Me, EventArgs.Empty)
        End Sub
#End Region

        Public Function Calculate(value As Integer) As String
            Return (value + _second).ToString()
        End Function
#End Region

        Public Default Property Item(index As Integer) As Integer
            Get
                Return If(index = 0, _first, _second)
            End Get
            Set(value As Integer)
                If index = 0 Then
                    _first = value
                Else
                    _second = value
                End If
            End Set
        End Property

        Public Custom Event Changed As EventHandler
            AddHandler(value As EventHandler)
            End AddHandler
            RemoveHandler(value As EventHandler)
            End RemoveHandler
            RaiseEvent()
            End RaiseEvent
        End Event
    End Class

    Public Structure Point
        Public X As Integer
        Public Y As Integer

        Public Shared Operator +(left As Point, right As Point) As Point
            Return New Point With {
                .X = left.X + right.X,
                .Y = left.Y + right.Y
            }
        End Operator
    End Structure

    Public Enum WorkState
        NotStarted
        Running
        Complete
    End Enum

    Public Delegate Function Transformer(value As Integer) As String

    Public Module NativeMethods
        Public Declare Function GetTickCount Lib "kernel32" () As UInteger
    End Module
End Namespace
