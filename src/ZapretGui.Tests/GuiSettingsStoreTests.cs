using System;
using System.IO;
using ZapretGui.Services;
using Xunit;

namespace ZapretGui.Tests;

public class GuiSettingsStoreTests
{
    [Fact]
    public void IsValidZapretPath_AllowsParenthesizedMask()
    {
        var tempDir = CreateTempDir();
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "general (ALT3).bat"), "echo test");
            var store = new GuiSettingsStore();

            var isValid = store.IsValidZapretPath(tempDir, "general (*.bat)", "service.bat");

            Assert.True(isValid);
        }
        finally
        {
            TryDelete(tempDir);
        }
    }

    [Fact]
    public void IsValidZapretPath_AllowsServiceScriptFallback()
    {
        var tempDir = CreateTempDir();
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "service.bat"), "echo service");
            var store = new GuiSettingsStore();

            var isValid = store.IsValidZapretPath(tempDir, "*.bat", "service.bat");

            Assert.True(isValid);
        }
        finally
        {
            TryDelete(tempDir);
        }
    }

    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "zapret-gui-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void TryDelete(string dir)
    {
        try
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, recursive: true);
            }
        }
        catch
        {
            // best-effort cleanup for CI environments
        }
    }
}
