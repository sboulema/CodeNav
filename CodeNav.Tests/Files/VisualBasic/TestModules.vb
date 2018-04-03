Module Module1

    ''' <summary>
    ''' Levels of importance.
    ''' </summary>
    Enum Importance
        None = 0
        Trivial = 1
        Regular = 2
        Important = 3
        Critical = 4
    End Enum

    Sub Main()
        Dim value As Importance = Importance.Critical
        ' Select the enum and print a value.
        Select Case value
            Case Importance.Trivial
                Console.WriteLine("Not true")
                Return
            Case Importance.Critical
                Console.WriteLine("True")
                Exit Select
        End Select
    End Sub

End Module