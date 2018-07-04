﻿using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test
{
    public class DumpEnvironmentTest
    {
        [Theory]
        [InlineData(null, 'D', false, MediaType.NONE, false)]
        [InlineData("", 'D', false, MediaType.NONE, false)]
        [InlineData("cd F test.bin 8 /c2 20", 'F', false, MediaType.CD, true)]
        [InlineData("fd A test.img", 'A', true, MediaType.Floppy, true)]
        [InlineData("dvd X test.iso 8 /raw", 'X', false, MediaType.Floppy, false)]
        [InlineData("stop D", 'D', false, MediaType.DVD, true)]
        public void IsConfigurationValidTest(string parameters, char letter, bool isFloppy, MediaType? mediaType, bool expected)
        {
            var env = new DumpEnvironment
            {
                DICParameters = parameters,
                Drive = isFloppy ? Drive.Floppy(letter) : Drive.Optical(letter, ""),
                Type = mediaType,
            };

            bool actual = env.IsConfigurationValid();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, null, null, new char(), null, null)]
        [InlineData("", null, null, new char(), null, null)]
        [InlineData("cd F test.bin 8 /c2 20", MediaType.CD, KnownSystem.IBMPCCompatible, 'F', "", "test.bin")]
        [InlineData("fd A blah\\test.img", MediaType.Floppy, KnownSystem.IBMPCCompatible, 'A', "blah", "test.img")]
        [InlineData("dvd X super\\blah\\test.iso 8 /raw", MediaType.GameCubeGameDisc, KnownSystem.NintendoGameCube, 'X', "super\\blah", "test.iso")]
        [InlineData("stop D", null, null, 'D', null, null)]
        public void AdjustForCustomConfigurationTest(string parameters, MediaType? expectedMediaType, KnownSystem? expectedKnownSystem, char expectedDriveLetter, string expectedOutputDirectory, string expectedOutputFilename)
        {
            var env = new DumpEnvironment
            {
                DICParameters = parameters,
                System = KnownSystem.Custom,
            };

            env.AdjustForCustomConfiguration();
            Assert.Equal(parameters, env.DICParameters);
            Assert.Equal(expectedMediaType, env.Type);
            Assert.Equal(expectedKnownSystem, env.System);
            Assert.Equal(expectedDriveLetter, env.Drive.Letter);
            Assert.Equal(expectedOutputDirectory, env.OutputDirectory);
            Assert.Equal(expectedOutputFilename, env.OutputFilename);
        }
        
        [Theory]
        [InlineData(null, null, null, null)]
        [InlineData(" ", "", " ", "")]
        [InlineData("super", "blah.bin", "super", "blah.bin")]
        [InlineData("super\\hero", "blah.bin", "super\\hero", "blah.bin")]
        [InlineData("super.hero", "blah.bin", "super_hero", "blah.bin")]
        [InlineData("superhero", "blah.rev.bin", "superhero", "blah_rev.bin")]
        [InlineData("super&hero", "blah.bin", "super_hero", "blah.bin")]
        [InlineData("superhero", "blah&foo.bin", "superhero", "blah_foo.bin")]
        public void FixOutputPathsTest(string outputDirectory, string outputFilename, string expectedOutputDirectory, string expectedOutputFilename)
        {
            var env = new DumpEnvironment
            {
                OutputDirectory = outputDirectory,
                OutputFilename = outputFilename,
            };

            env.FixOutputPaths();
            Assert.Equal(expectedOutputDirectory, env.OutputDirectory);
            Assert.Equal(expectedOutputFilename, env.OutputFilename);
        }
    }
}
