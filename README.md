# MyInstallManager

To set up the demo:

1. Compile `SampleServer\public\updater-1.0.0.c` into an EXE in the same
   directory. Under MinGW/MSYS2, I ran
   ```
   gcc updater-1.0.0.c -o updater-1.0.0.exe -municode
   ```
   and under MSVC/VisualStudio it'd look something like
   ```
   cl updater-1.0.0.c /o updater-1.0.0.exe
   ```
   in the Developer Command Prompt.

2. Compile the project with `dotnet build`.

3. Run an HTTP server for `SampleServer\public`. Officially:
   ```
   cd SampleServer # (do these in a new terminal)
   npm install
   node index.js
   ```
   Unofficially, `cd SampleServer/public && python3 -m http.server 3000` :)

4. Find the compiled installer, I'll go with the Stub Installer.
   On my system it's in `StubInstaller\bin\debug\net10.0-windows\StubInstaller.exe`.
   I'll refer to it as `<cmd>`.

5. Run `<cmd> install --dir install-tmp` to 'install' the program.
   Then, run `<cmd> update --dir install-tmp` to update it.

