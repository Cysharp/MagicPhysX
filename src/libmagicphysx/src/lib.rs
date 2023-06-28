#[allow(dead_code)]
#[allow(non_snake_case)]
#[allow(non_camel_case_types)]
#[allow(non_upper_case_globals)]
#[allow(improper_ctypes)]
use physx_sys;

#[allow(dead_code)]
#[allow(non_snake_case)]
#[allow(non_camel_case_types)]
#[allow(non_upper_case_globals)]
#[allow(improper_ctypes)]
mod physx_ffi;

#[cfg(test)]
mod tests {
    use std::{
        env,
        fs::{self},
        io::Write,
    };

    use regex::Regex;

    // cargo test update_package_version -- 1.0.0 --nocapture
    #[test]
    fn update_package_version() {
        let args: Vec<String> = env::args().collect();
        // 0: exe path
        // 1: update_package_version
        // 2: 1.0.0
        // 3: --nocapture

        if args[1] != "update_package_version" {
            return;
        }

        println!("version: {}", args[2]);
        let mut path = std::env::current_dir().unwrap();
        println!("current_dir: {}", path.display());

        path.push("Cargo.toml");
        let toml = fs::read_to_string(path.clone()).unwrap();

        // replace only first-match
        let regex = Regex::new("physx-sys = \".+\"").unwrap();

        let new_toml = regex.replace(toml.as_str(), format!("physx-sys = \"{}\"", args[2]));

        let mut file = fs::File::create(path.clone()).unwrap();
        file.write_all(new_toml.as_bytes()).unwrap();
        file.flush().unwrap();
    }

    // cargo test move_target_lib -- x86_64-pc-windows-msvc win-x64 --nocapture
    #[test]
    fn move_target_lib() {
        let args: Vec<String> = env::args().collect();
        // 0: exe path
        // 1: move_target_lib
        // 2: x86_64-pc-windows-msvc
        // 3: win-x64
        // 3: --nocapture

        if args[1] != "move_target_lib" {
            return;
        }

        println!("target: {}", args[2]);
        let mut path = std::env::current_dir().unwrap();
        println!("current_dir: {}", path.display());

        let ext = if args[3].contains("linux-") {
            "so"
        } else if args[3].contains("osx-") {
            "dylib"
        } else {
            "dll"
        };

        path.push(format!("target/{}/release/", args[2]));

        // print directory-files for CI debugging
        let paths = fs::read_dir(path.clone()).unwrap();
        for p in paths {
            println!("File: {}", p.unwrap().path().display())
        }

        path.push(format!("libmagicphysx.{}", ext));

        let mut to = std::env::current_dir().unwrap();
        to.push(format!(
            "../MagicPhysX/runtimes/{}/native/libmagicphysx.{}",
            args[3], ext
        ));

        println!("move from: {} to: {}", path.display(), to.display());
        std::fs::rename(path, to).unwrap();
    }
}
