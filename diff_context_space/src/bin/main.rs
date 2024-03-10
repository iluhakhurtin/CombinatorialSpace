use std::fs::File;
use std::path::PathBuf;
use std::io::Write;

extern crate diff_context_space;
use diff_context_space::{Transformation, Information, ContextSpace, Context};

extern crate num_traits;
use num_traits::int::PrimInt;

extern crate bincode;

extern crate serde;
use serde::{Serialize};

extern crate image;
use image::imageops::{resize, FilterType};

// to pass arguments with cargo: cargo run -- load -t cs -f 'some file to load'

fn main() {
    // generate_and_save_64x64_xya_transformations();
    // transform_32x32_to_64x64();
    // teach_context_space_by_64x64_cc_mono_icon_set_and_save();
    load_context_space_and_learn_new_information();
    // transform_and_save_image();
    // print_transformations();
}

fn generate_and_save_64x64_xya_transformations() {
    let env_path = env!("CARGO_MANIFEST_DIR");
    let mut out_file_path = PathBuf::from(env_path);
    out_file_path.push("files/transformations/t_64x64.bin");
    // 32 because transformations for 64x64 are half of possible movements
    diff_context_space::build_and_save_transformations(out_file_path, 32, false);
}

/// Transforms existing images from 32x32 folder into 
/// 64x64 saving them into the appropriate folder.
fn transform_32x32_to_64x64() {
    let env_path = env!("CARGO_MANIFEST_DIR");
    
    let mut i_path = PathBuf::from(env_path);
    i_path.push("files/interpretations/32x32/cc_mono_icon_set/");
    let dir_entries = std::fs::read_dir(i_path).unwrap();

    let mut o_path = PathBuf::from(env_path);
    o_path.push("files/interpretations/64x64/cc_mono_icon_set/");

    const WIDTH:u32 = 64;
    const HEIGHT:u32 = WIDTH;

    for dr in dir_entries {
        let dir_entry = dr.unwrap();
        let file_path = dir_entry.path();
        
        match image::open(&file_path) {
            Ok(img) => {
                let i = resize(&img, WIDTH, HEIGHT, FilterType::Nearest);

                let mut full_out_path = o_path.clone();
                full_out_path.push(dir_entry.file_name());
                i.save_with_format(full_out_path, image::ImageFormat::Png).expect("Unable to save image file");

                println!("Processed: {}", &file_path.into_os_string().into_string().unwrap());
            },
            Err(e) => {
                println!("{}: {}", e, &file_path.into_os_string().into_string().unwrap());
            }
        }
    }
}

fn load_context_space_and_learn_new_information() {
    let env_path = env!("CARGO_MANIFEST_DIR");
    let mut cs_path = PathBuf::from(env_path);
    cs_path.push("files/out/context_spaces/cs_64x64_mono_icon_set.bin");

    let now = std::time::SystemTime::now();

    let mut cs = ContextSpace::<u64>::load(cs_path).unwrap();

    println!("Loading context space 64x64 took {} seconds.", now.elapsed().unwrap().as_secs());

    // load transformations

    let now = std::time::SystemTime::now();

    let mut ts_path = PathBuf::from(env_path);
    ts_path.push("files/transformations/t_64x64.bin");
    let ts = load_transformations(&ts_path);

    println!("N of transformations: {}", ts.len());

    println!("Loading transformations t_64x64 took {} seconds.", now.elapsed().unwrap().as_secs());

    // create log file
    let mut log_path = PathBuf::from(env_path);
    log_path.push("files/out/logs/int_log_64x64.log");
    let mut log_file = File::create(log_path).unwrap();
    
    for idx in 1..2 {
        // take image from galaxy
        let mut galaxy_img_path = PathBuf::from(env_path);
        galaxy_img_path.push(format!("files/interpretations/64x64/galaxy/{}.png", idx));

        let img = image::open(galaxy_img_path).unwrap();
        let expected_name = format!("{}.png", idx);
        let int = Information::<u64>::from_image(&img, expected_name.clone());

        // teach context space with new image, no transformation
        let t = Transformation { y: 0, x: 0, a: 0.0 };
        cs.learn(&t, &int, int.clone());

        for t in &ts {
            // transform image
            let i = t.apply_to(&int);

            match cs.interpret(&i, 0.9) {
                None => {
                    writeln!(&mut log_file, "Interpretation could not been found, transformation: {}.", t).unwrap()
                },
                Some(data) => {
                    // //save existing interpretation image
                    // let mut existing_int_out_path = PathBuf::from(env_path);
                    // existing_int_out_path.push(format!("files/out/interpretations/{}_{}_existing.png", idx, t));
                    // let existing_int_img = data.0.to_image();
                    // existing_int_img.save_with_format(existing_int_out_path, image::ImageFormat::Png).expect("Unable to save image file");

                    // //save actual image
                    // let mut actual_int_out_path = PathBuf::from(env_path);
                    // actual_int_out_path.push(format!("files/out/interpretations/{}_{}_actual.png", idx, t));
                    // let actual_int_img = data.3.to_image();
                    // actual_int_img.save_with_format(actual_int_out_path, image::ImageFormat::Png).expect("Unable to save image file");

                    //print interpretation info
                    let t_selected = data.1;
                    let int_existing = data.0;
                    let int_actual = data.3;
                    let accuracy = data.2;
                    let coherence = int_existing.coherence_to(&int_actual).unwrap();
                    let t_match = t_selected == *t;
                    let int_match = int_existing.name == expected_name;

                    let tdx = (t_selected.x - t.x).abs();
                    let tdy = (t_selected.y - t.y).abs();
                    let tda = (t_selected.a - t.a).abs();

                    writeln!(&mut log_file, "int: {}, sel int: {}, int_match: {}, t_match: {}, acc: {}, coh: {}, t: {}, sel_t: {}, t_dx: {}, t_dy: {}, t_da: {}.",
                        expected_name.clone(),
                        int_existing.name,
                        int_match,
                        t_match,
                        accuracy,
                        coherence,
                        t,
                        t_selected,
                        tdx,
                        tdy,
                        tda
                        ).unwrap();
                }
            }
        }
    }
}

fn teach_context_space_by_64x64_cc_mono_icon_set_and_save() {
    let now = std::time::SystemTime::now();
    
    let env_path = env!("CARGO_MANIFEST_DIR");

    let mut ts_path = PathBuf::from(env_path);
    ts_path.push("files/transformations/t_64x64.bin");
    let ts = load_transformations(&ts_path);

    let mut cs = ContextSpace::<u64>::new();

    let mut img_folder_path = PathBuf::from(env_path);
    img_folder_path.push("files/interpretations/64x64/cc_mono_icon_set/");
    let dir_entries = std::fs::read_dir(img_folder_path).unwrap();

    println!("Loaded transformations and reading images to learn took {} seconds.", now.elapsed().unwrap().as_secs());
    let now = std::time::SystemTime::now();

    for dr in dir_entries {
        let dir_entry = dr.unwrap();
        let img_path = dir_entry.path();

        let name = String::from(img_path.file_name().unwrap().to_str().unwrap());
        let img = image::open(img_path).unwrap();
        let int = Information::<u64>::from_image(&img, name);

        for t in &ts {
            let i = t.apply_to(&int);
            cs.learn(t, &i, int.clone());
        }
    }

    println!("Learning took {} seconds.", now.elapsed().unwrap().as_secs());

    let all_count = cs.contexts.len();
    let active_count = cs.contexts.iter().filter(|c| c.rules.len() > 0).count();
    println!("All contexts count: {}, active: {}, transformations count: {}.", all_count, active_count, &ts.len());

    // print_contexts(cs.contexts.iter());

    let now = std::time::SystemTime::now();

    let mut out_cs_path = PathBuf::from(env_path);
    out_cs_path.push("files/out/context_spaces/cs_64x64_mono_icon_set.bin");
    cs.save(out_cs_path).unwrap();

    println!("Saving of the context space file took {} seconds.", now.elapsed().unwrap().as_secs());
}

fn print_contexts<'a, I, T>(contexts: I)
    where T: 'a + PrimInt + Serialize,
    I: Iterator<Item = &'a Context<T>>, {

    let contexts_to_print = contexts.filter(|c| c.rules.len() > 0);

    let env_path = env!("CARGO_MANIFEST_DIR");
    let mut out_rules_path = PathBuf::from(env_path);
    out_rules_path.push("files/out/contexts/");

    let mut full_out_path = PathBuf::from(&out_rules_path);
    full_out_path.push("all_contexts_rules.gif");
    let file_buff = File::create(full_out_path).expect("Unable to create the output file.");
    let mut gif_enc = image::gif::GifEncoder::new(file_buff);

    let mut active_count = 0;

    for c in contexts_to_print {
        active_count = active_count + 1;
        for r_idx in 0..c.rules.len() {
            let img = c.rules[r_idx].to_image();
            let img_frame = image::Frame::new(img.to_rgba());
            gif_enc.encode_frame(img_frame).unwrap();
        }
    }
}

fn load_transformations(path: &PathBuf) -> Vec<Transformation> {
    let f = File::open(path)
                .expect("Unable to open the file.");

    let transformations = bincode::deserialize_from(f)
                                .expect("Unable to read binary transformations from the file.");
    transformations
}

fn transform_and_save_image() {
    let env_path = env!("CARGO_MANIFEST_DIR");
    
    let mut orig_img_path = PathBuf::from(env_path);
    orig_img_path.push("files/interpretations/32x32/galaxy/1.png");

    let img = image::open(orig_img_path).unwrap();
    let int = Information::<u32>::from_image(&img, String::from("1.png"));

    let t = Transformation { x: -5, y: 5, a: 0.0 };
    let i = t.apply_to(&int);
    let out_img = i.to_image();

    let mut out_img_path = PathBuf::from(env_path);
    out_img_path.push("files/out/images/1.png");

    out_img.save_with_format(out_img_path, image::ImageFormat::Png).unwrap();
}

fn print_transformations() {
    let env_path = env!("CARGO_MANIFEST_DIR");

    let mut ts_path = PathBuf::from(env_path);
    ts_path.push("files/transformations/t_64x64.bin");
    let ts = load_transformations(&ts_path);

    let mut log_path = PathBuf::from(env_path);
    log_path.push("files/out/logs/t_log.log");

    let mut log_file = File::create(log_path).unwrap();

    for t in ts {
        writeln!(&mut log_file, "{}", t);
    }
}