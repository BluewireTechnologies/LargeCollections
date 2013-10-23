using System;
using System.IO;

namespace LargeCollections.Resources.Diagnostics
{
    class CrashLog
    {
        private string crashLogDirectory;

        public CrashLog(string crashLogDirectory)
        {
            if (!Directory.Exists(crashLogDirectory))
            {
                // Create the directory here rather than waiting until we crash.
                Directory.CreateDirectory(crashLogDirectory);
            }
            this.crashLogDirectory = crashLogDirectory;
        }

        private void WithLog(Action<TextWriter> write)
        {
            var fileName = String.Format("crash-{0}", DateTime.Now.ToString("yyyyMMdd-HHmmss.fff"));
            using (var writer = File.CreateText(Path.Combine(crashLogDirectory, fileName)))
            {
                write(writer);
            }
        }

        private void WriteExceptionDetails(TextWriter writer, Exception exception, ReferenceCountedResource resource)
        {
            writer.WriteLine("Resource was not disposed: {0}", resource);
            writer.WriteLine("An exception was thrown during cleanup on the finaliser thread, causing a crash.");
            writer.WriteLine();
            writer.WriteLine(exception.Message);
            writer.WriteLine(exception.StackTrace);
        }

        private void WriteReferenceCount(TextWriter writer, int count)
        {
            writer.WriteLine("The resource had {0} live references.", count);
        }

        private void WriteReferenceDetails(TextWriter writer, ITracedReference reference)
        {
            var trace = reference.GetAcquisitionTrace();
            writer.WriteLine(trace ?? "(no trace available)");
        }

        public void Log(Exception exception, ReferenceCountedResource resource, int referenceCount)
        {
            WithLog(w =>
            {
                WriteExceptionDetails(w, exception, resource);
                w.WriteLine();
                WriteReferenceCount(w, referenceCount);
            });
        }

        public void Log(Exception exception, ReferenceCountedResource resource, ITracedReference[] references)
        {
            WithLog(w =>
            {
                WriteExceptionDetails(w, exception, resource);
                w.WriteLine();
                WriteReferenceCount(w, references.Length);
                foreach (var reference in references)
                {
                    w.WriteLine();
                    WriteReferenceDetails(w, reference);
                }
            });
        }
    }
}