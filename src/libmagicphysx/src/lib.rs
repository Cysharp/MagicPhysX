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
}
