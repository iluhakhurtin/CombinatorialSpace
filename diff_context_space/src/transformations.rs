use std::fs::File;
use std::path::PathBuf;
use std::f32::consts::{PI, FRAC_PI_8, FRAC_PI_4, FRAC_PI_2};

use crate::{ Transformation };

pub fn build_and_save_transformations(out_file_path: PathBuf, size: u8, use_rotation: bool) {
    // generate transformations for every shift left/right/up/down

    let t_count:usize = size as usize * size as usize;
    let mut transformations = Vec::<Transformation>::with_capacity(t_count);

    let max_x_shift = ((size / 2) + 1) as i16;
    let max_y_shift = ((size / 2) + 1) as i16;

    for y_shift in 0..max_y_shift {
        for x_shift in 0..max_x_shift {
            let n_y_shift = -1 * y_shift;
            let n_x_shift = -1 * x_shift;

            let mut ts = get_transformations(x_shift, y_shift, use_rotation);
            transformations.append(&mut ts);

            if n_y_shift != 0 {
                let mut ts = get_transformations(x_shift, n_y_shift, use_rotation);
                transformations.append(&mut ts);
            }
            
            if n_x_shift != 0 {
                let mut ts = get_transformations(n_x_shift, y_shift, use_rotation);
                transformations.append(&mut ts);
            }            

            if n_y_shift != 0 && n_x_shift != 0 {
                let mut ts = get_transformations(n_x_shift, n_y_shift, use_rotation);
                transformations.append(&mut ts);
            }
        }
    }

    println!("Generated {} transformations.", transformations.len());

    let f = File::create(out_file_path)
                    .expect("Unable to create the file.");

    bincode::serialize_into(f, &transformations)
        .expect("Unable to write json to the file.");
}

/// Builds all rotation transformations for the given xy.
/// The expected rotations are:
/// 0       = 0 deg
/// Pi/8    = 22.5 deg
/// Pi/4    = 45 deg
/// 3Pi/8   = 67.5 deg
/// Pi/2    = 90 deg
/// 5Pi/8   = 112.5 deg
/// 3Pi/4   = 135 deg
/// 7Pi/8   = 157.5 deg
/// Pi is removed since it is equal to 0
fn get_transformations(x: i16, y: i16, use_rotation: bool) -> Vec<Transformation> {
    let mut r = Vec::new();
    
    // 0
    let t = Transformation {x, y, a: 0.0};
    r.push(t);

    if !use_rotation {
        return r;
    }

    // // Pi/8
    // let t = Transformation {x, y, a: FRAC_PI_8};
    // r.push(t);

    // Pi/4
    let t = Transformation {x, y, a: FRAC_PI_4};
    r.push(t);
    
    // // 3Pi/8
    // let t = Transformation {x, y, a: 3.0 * FRAC_PI_8};
    // r.push(t);

    // Pi/2
    let t = Transformation {x, y, a: FRAC_PI_2};
    r.push(t);

    // // 5Pi/8
    // let t = Transformation {x, y, a: 5.0 * FRAC_PI_8};
    // r.push(t);

    // 3Pi/4
    let t = Transformation {x, y, a: 3.0 * FRAC_PI_4};
    r.push(t);

    // // 7Pi/8
    // let t = Transformation {x, y, a: 7.0 * FRAC_PI_8};
    // r.push(t);

    r
}