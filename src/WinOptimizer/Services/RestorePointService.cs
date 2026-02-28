namespace WinOptimizer.Services;

using System.Management;
using WinOptimizer.Helpers;

public static class RestorePointService
{
    public static bool CreateRestorePoint(string description)
    {
        try
        {
            LogHelper.Log($"Creating restore point: {description}");
            var restorePointClass = new ManagementClass(
                "\\\\.\\root\\default", "SystemRestore", new ObjectGetOptions());
            var inParams = restorePointClass.GetMethodParameters("CreateRestorePoint");
            inParams["Description"] = description;
            inParams["RestorePointType"] = 12; // MODIFY_SETTINGS
            inParams["EventType"] = 100;       // BEGIN_SYSTEM_CHANGE
            var outParams = restorePointClass.InvokeMethod("CreateRestorePoint", inParams, null);
            var returnValue = Convert.ToInt32(outParams["ReturnValue"]);

            if (returnValue == 0)
            {
                LogHelper.Log("Restore point created successfully");
                return true;
            }

            LogHelper.Log($"Restore point creation failed with code: {returnValue}");
            return false;
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error creating restore point: {ex.Message}");
            return false;
        }
    }

    public static bool PromptAndCreate(string description)
    {
        var result = MessageBox.Show(
            "Do you want to create a System Restore point before proceeding?\n\nThis is recommended so you can undo changes if needed.",
            "Create Restore Point",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
            return true; // User chose to skip, not an error

        var success = CreateRestorePoint(description);
        if (!success)
        {
            var proceed = MessageBox.Show(
                "Failed to create a restore point. Do you want to continue anyway?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            return proceed == DialogResult.Yes;
        }

        return true;
    }
}
