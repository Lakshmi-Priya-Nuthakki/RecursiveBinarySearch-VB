Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Runtime.Remoting.Lifetime
Imports System.Web

Public Class Form1
    Private ReadOnly strlist As New List(Of String)()
    Private loadedStrCount As Integer = 0

    Private issorted As Boolean = False

    Private Sub rdostring_checkedchanged(sender As Object, e As EventArgs)
        issorted = False
        lblResult.Text = ""
        txtSearch.Clear()
        RefreshListBox()
    End Sub

    Private Sub btnadd_click(sender As Object, e As EventArgs) Handles btnAdd.Click
        Try
            Dim raw As String = txtValue.Text.Trim()

            If String.IsNullOrWhiteSpace(raw) Then
                MessageBox.Show("Please enter a value.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim parts As String()
            If raw.Contains(","c) Then
                parts = raw.Split(","c)
            Else
                parts = New String() {raw}
            End If

            For Each p In parts
                Dim s As String = p.Trim()
                If s = "" Then Continue For

                If String.IsNullOrWhiteSpace(s) Then
                    MessageBox.Show("Please enter a valid string", "Invalide input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
                strlist.Add(s)
            Next

            issorted = False
            txtSearch.Clear()
            lblResult.Text = ""
            RefreshListBox()

        Catch ex As Exception
            MessageBox.Show($"Unexpected error while adding: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnsort_click(sender As Object, e As EventArgs) Handles btnSort.Click
        Try
            strlist.Sort(StringComparer.OrdinalIgnoreCase)

            issorted = True
            lblResult.Text = "Sorted"
            RefreshListBox()
        Catch ex As Exception
            MessageBox.Show($"Sort failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try
    End Sub

    Private Function Recursivebinarysearch(Of T)(list As List(Of T), low As Integer, high As Integer, target As T, comparer As IComparer(Of T)) As Integer
        If low > high Then Return -1

        Dim mid As Integer = (low + high) \ 2
        Dim cmp As Integer = comparer.Compare(list(mid), target)

        If cmp = 0 Then
            Return mid
        ElseIf cmp > 0 Then
            Return Recursivebinarysearch(list, low, mid - 1, target, comparer)
        Else
            Return Recursivebinarysearch(list, mid + 1, high, target, comparer)
        End If
    End Function

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Try
            If Not issorted Then
                MessageBox.Show("Please sort the list before search.", "Sort Required", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            Dim raw As String = txtSearch.Text.Trim()

            If String.IsNullOrWhiteSpace(raw) Then
                MessageBox.Show("Enter a string to search.", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim comp As IComparer(Of String) = StringComparer.OrdinalIgnoreCase
                Dim idx = Recursivebinarysearch(strlist, 0, strlist.Count - 1, raw, comp)
            ShowSearchResult(idx)

        Catch ex As Exception
            MessageBox.Show($"Search Filed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ShowSearchResult(index As Integer)
        If index >= 0 Then
            lblResult.Text = $"Element Found at index {index}."
            lstItems.SelectedIndex = index
        Else
            lblResult.Text = "Element Not Found."
            lstItems.ClearSelected()
        End If
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            Using sfd As New SaveFileDialog()
                sfd.Filter = "Text Files (*.txt) | *.txt|All Files (*.*)|*.*"
                sfd.Title = "Save List"

                If sfd.ShowDialog() <> DialogResult.OK Then Return

                Dim writeHeader As Boolean = Not File.Exists(sfd.FileName) OrElse New FileInfo(sfd.FileName).Length = 0


                Using writer As New StreamWriter(sfd.FileName, append:=True)
                    If writeHeader Then
                        writer.WriteLine("TYPE=STRING")
                    End If

                    For i As Integer = loadedStrCount To strlist.Count - 1
                        writer.WriteLine(strlist(i))
                    Next
                    loadedStrCount = strlist.Count
                End Using
            End Using

            MessageBox.Show("saved successfully.", "saved", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show($"Save Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnLoad_Click(sender As Object, e As EventArgs) Handles btnLoad.Click
        Try
            Using ofd As New OpenFileDialog()
                ofd.Filter = "Text Files (*.txt) | *.txt|All Files (*.*)|*.*"
                ofd.Title = "Save List"

                If ofd.ShowDialog() <> DialogResult.OK Then Return

                Dim lines = File.ReadAllLines(ofd.FileName).ToList()
                If lines.Count = 0 Then
                    MessageBox.Show("File is empty.", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If

                Dim header As String = lines(0).Trim().ToUpperInvariant

                strlist.Clear()

                If header = "TYPE=STRING" Then
                    For i As Integer = 1 To lines.Count - 1
                        Dim t = lines(i).Trim()
                        If t <> "" Then
                            strlist.Add(t)
                        End If
                    Next
                Else
                    MessageBox.Show("Unknown file format,first line must be TYPE=INT or TYPE=STRING.", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If

                loadedStrCount = strlist.Count

                issorted = False
                lblResult.Text = "Loaded. Sort before searching."
                RefreshListBox()
            End Using
        Catch ex As Exception
            MessageBox.Show($"Load failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub RefreshListBox()
        lstItems.Items.Clear()

        For Each s In strlist
            lstItems.Items.Add(s)
        Next
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class