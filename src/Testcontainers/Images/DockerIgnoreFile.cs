namespace DotNet.Testcontainers.Images
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using Microsoft.Extensions.Logging;

  /// <summary>
  /// An implementation of <see cref="IgnoreFile" /> that uses the patterns of the .dockerignore file to ignore directories and files.
  /// </summary>
  internal sealed class DockerIgnoreFile : IgnoreFile
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DockerIgnoreFile" /> class.
    /// </summary>
    /// <param name="dockerignoreFileDirectory">Directory that contains all docker configuration files.</param>
    /// <param name="dockerignoreFileExtension">.dockerignore file extension.</param>
    /// <param name="dockerfileFile">Dockerfile file name.</param>
    /// <param name="logger">The logger.</param>
    public DockerIgnoreFile(string dockerignoreFileDirectory, string dockerignoreFileExtension, string dockerfileFile, ILogger logger)
      : this(new DirectoryInfo(dockerignoreFileDirectory), dockerignoreFileExtension, dockerfileFile, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockerIgnoreFile" /> class.
    /// </summary>
    /// <param name="dockerignoreFileDirectory">Directory that contains all docker configuration files.</param>
    /// <param name="dockerignoreFileExtension">.dockerignore file extension.</param>
    /// <param name="dockerfileFile">Dockerfile file name.</param>
    /// <param name="logger">The logger.</param>
    public DockerIgnoreFile(FileSystemInfo dockerignoreFileDirectory, string dockerignoreFileExtension, string dockerfileFile, ILogger logger)
      : base(GetPatterns(dockerignoreFileDirectory, dockerignoreFileExtension, dockerfileFile), logger)
    {
    }

    private static IEnumerable<string> GetPatterns(FileSystemInfo dockerignoreFileDirectory, string dockerignoreFileExtension, string dockerfileFile)
    {
      var customDockerIgnoreFilePath = Path.Combine(dockerignoreFileDirectory.FullName, dockerfileFile + dockerignoreFileExtension);

      var dockerignoreFilePath = File.Exists(customDockerIgnoreFilePath)
        ? customDockerIgnoreFilePath
        : Path.Combine(dockerignoreFileDirectory.FullName, dockerignoreFileExtension);

      // These files are necessary and sent to the Docker daemon. The ADD and COPY instructions do not copy them to the image:
      // https://docs.docker.com/engine/reference/builder/#dockerignore-file.
      var negateNecessaryFiles = new[] { dockerignoreFileExtension, dockerfileFile }
        .Select(file => "!" + file);

      var dockerignorePatterns = File.Exists(dockerignoreFilePath)
        ? File.ReadLines(dockerignoreFilePath)
        : Array.Empty<string>();

      return new[] { "**/.idea", "**/.vs" }
        .Concat(dockerignorePatterns)
        .Concat(negateNecessaryFiles);
    }
  }
}
