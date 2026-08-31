# Troubleshooting CodeNav

If you're experiencing issues with CodeNav, there are two main ways to help us troubleshoot and resolve problems:

## 1. Enable Crash Analytics (Recommended for Automatic Reporting)

The easiest way to help us fix bugs is to enable crash analytics. This automatically sends error information to Application Insights whenever CodeNav encounters an exception.

### How to Enable Crash Analytics

1. Open CodeNav and go to **Settings**
3. Under the **Analytics** section, check the box for **"Enable sending crash analytics to Application Insights"**
4. Click **OK** to save

![Analytics Setting in CodeNav Settings](../art/Settings.png)

Once enabled, any crashes or exceptions will be automatically reported to Application Insights. This helps the CodeNav team identify and fix issues much faster.

### What Gets Sent

When crash analytics is enabled, CodeNav sends:
- **Exception type and message** - What went wrong
- **Stack trace** - Where in the code the error occurred
- **CodeNav version** - Which version you're running
- **Timestamp** - When the error occurred

**Note:** No personal data, file contents, or code snippets are sent. Only error information is collected.

---

## 2. Manual Troubleshooting via Visual Studio Output Window

If you prefer not to enable automatic analytics, or if you want to provide more detailed information for a GitHub issue, you can manually collect error logs from the Output window.

### How to Collect Error Messages and Stack Traces

1. In Visual Studio, open the **Output** window (**View** → **Output** or `Ctrl + Alt + O`)
2. In the **"Show output from:"** dropdown (top-left of the Output window), select **"CodeNav"**
3. Reproduce the issue or error you're experiencing
4. Copy the error messages and stack traces from the Output window

### Example Output

The Output window will show logs like:
```
CodeNav: Error processing file: MyClass.cs
System.NullReferenceException: Object reference not set to an instance of an object.
   at CodeNav.OutOfProc.Services.CodeDocumentService.UpdateCodeDocumentViewModel()
   at CodeNav.OutOfProc.TextViewEventListener.OnTextViewChanged()
```

### Reporting the Issue on GitHub

When you create a [GitHub issue](https://github.com/sboulema/CodeNav/issues), please include:

1. **Description** - What were you doing when the error occurred?
2. **Reproduction Steps** - How can we reproduce the issue?
3. **Error Messages** - The full error output from the CodeNav Output window
4. **Stack Trace** - The complete stack trace
5. **Environment**
   - Visual Studio version
   - CodeNav version
   - File type you were working with (C#, VB.NET, TypeScript, etc.)

### Example Issue Template

```markdown
## Description
CodeNav crashed when I opened a large C# file with many nested classes.

## Reproduction Steps
1. Open Visual Studio
2. Open the file `MyLargeFile.cs` (attached or link to repo)
3. CodeNav shows an error in the Output window

## Error Output
CodeNav: Error updating CodeDocumentViewModel
System.ArgumentOutOfRangeException: Index was out of range...
   at [stack trace here]

## Environment
- Visual Studio: 2022 (v17.x)
- CodeNav: v10.x
- File Type: C#
```

---

## Tips for Better Troubleshooting

- **Check the Output window regularly** - Errors often appear in the CodeNav output channel before the UI is affected
- **Enable analytics early** - If you enable it right away, future errors will be automatically reported
- **Provide context** - The more detail you provide about what you were doing, the easier it is for us to fix
- **Test with a minimal file** - If possible, create a small reproduction case to help us isolate the issue

---

## Still Need Help?

- Check existing [GitHub issues](https://github.com/sboulema/CodeNav/issues) to see if your problem has been reported
- Create a new [GitHub issue](https://github.com/sboulema/CodeNav/issues/new) with the details above
- Include the CodeNav version number in your report

Thank you for helping make CodeNav better! 🙏
