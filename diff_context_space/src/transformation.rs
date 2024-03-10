use crate::Information;

use std::fmt;
use std::f32::consts::{PI, FRAC_PI_6, FRAC_PI_3, FRAC_PI_2};

extern crate num_traits;
use num_traits::int::PrimInt;

extern crate serde;
use serde::{Serialize, Deserialize};

extern crate image;
use image::{DynamicImage, Luma};

extern crate imageproc;
use imageproc::geometric_transformations::{rotate_about_center, translate, Interpolation};

/// This structure is to store the transformation
/// applied by the context.
/// x is horizontal, left shift is negative, right is positive.
/// y is vertical, up is positive, down is negative.
/// a is an angle to rotate counter clockwise about the center in radians.
#[derive(Clone, Serialize, Deserialize)]
pub struct Transformation {
    pub x: i16,
    pub y: i16,
    pub a: f32,
}

impl Transformation {
    /// Calculates distance value to another transformation. 
    /// This method allows to measure how close is one transformation 
    /// to another.
    /// TODO: add logic to have seamless distance calculation for the
    /// transformations located on the edges. The transformation on the left
    /// top corner should be considered with small distance to the transformation
    /// located on the bottom left corner and bottom right corner and top right 
    /// corner.
    /// TODO: upgrade the logic to take rotation a into account
    pub fn distance_to(&self, to: &Transformation) -> f32 {
        let mut dh = to.x - self.x;
        dh *= dh;

        let mut dv = to.y - self.y;
        dv *= dv;

        let mut da = to.a - self.a;
        da *= da;

        let mut d = (dh + dv) as f32;
        d += da;

        let r = d.sqrt();
        r
    }

    pub fn apply_to<T: PrimInt + Serialize>(&self, to: &Information<T>) -> Information<T> {
        let img = to.to_image().to_luma8();

        // since vertical for translate function goes from up to down
        // (it is common for computer graphics),
        // but has opposite direction for xy coordinates of a paper sheet
        // vertical is multiplied on -1.
        let t = (self.x as i32, (self.y * -1) as i32);
        let mut img = translate(&img, t);

        if self.a != 0.0 {
            let default = Luma([0]);        // black
            let theta = 2.0 * PI - self.a;  // positive rotation for the function is clockwise,
                                            // but on a paper sheet is counter clockwise
            img = rotate_about_center(&img, theta, Interpolation::Nearest, default);
        }
        
        let out_img = DynamicImage::ImageLuma8(img);
        let name = to.name.clone();
        Information::from_image(&out_img, name)
    }
}

impl fmt::Display for Transformation {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        write!(f, "x: {} y: {} a: {}", 
                self.x,
                self.y,
                self.a)
    }
}

impl std::cmp::PartialEq for Transformation {
    fn eq(&self, other: &Self) -> bool {
        self.y == other.y && self.x == other.x
    }
}

#[test]
fn distance_test() {
    let t1 = Transformation { x: 1, y: 1, a: 0.0 };
    let t2 = Transformation { x: 1, y: 3, a: 0.0 };

    let d = t1.distance_to(&t2);
    assert_eq!(d, 2.0);

    let t2 = Transformation { x: 3, y: 1, a: 0.0 };
    let d = t1.distance_to(&t2);
    assert_eq!(d, 2.0);

    let t2 = Transformation { x: 2, y: 2, a: 0.0 };
    let d = t1.distance_to(&t2);
    assert_eq!(d, 1.4142135);

    let t2 = Transformation { x: 3, y: 3, a: 0.0 };
    let d = t1.distance_to(&t2);
    assert_eq!(d, 2.828427);
}

#[test]
fn can_shift_h_right() {
    let t = Transformation { x: 1, y: 0, a: 0.0 };

    let i = Information { data: Vec::<i8>::from([
        0b_0010_0000,
        0b_0010_0000,
        0b_0010_0000
    ])};

    let int = t.apply_to(&i);

    assert_eq!(int.data, Vec::<i8>::from([
        0b_0001_0000,
        0b_0001_0000,
        0b_0001_0000
    ]));
}

#[test]
fn can_shift_h_left() {
    let t = Transformation { x: -1, y: 0, a: 0.0 };
    let i = Information { data: vec![
        0b_0010_0000,
        0b_0010_0000,
        0b_0010_0000
    ]};

    let int = t.apply_to(&i);

    assert_eq!(int.data, vec![
        0b_0100_0000,
        0b_0100_0000,
        0b_0100_0000
    ]);
}

#[test]
fn can_shift_vertical_up() {
    let t = Transformation { x: 0, y: 1, a: 0.0 };
    let i = Information { data: Vec::<i8>::from([
        0b_0000_0000,
        0b_0010_0000,
        0b_0000_0000
    ])};

    let int = t.apply_to(&i);

    assert_eq!(int.data, Vec::<i8>::from([
        0b_0010_0000,
        0b_0000_0000,
        0b_0000_0000
    ]));
}

#[test]
fn can_shift_vertical_down() {
    let t = Transformation { x: 0, y: -1, a: 0.0 };
    let i = Information { data: vec![
        0b_0000_0000,
        0b_0010_0000,
        0b_0000_0000
    ]};

    let int = t.apply_to(&i);

    assert_eq!(int.data, vec![
        0b_0000_0000,
        0b_0000_0000,
        0b_0010_0000
    ]);
}

#[test]
fn can_shift_vertical_up_and_h_right() {
    let t = Transformation { x: 1, y: 1, a: 0.0 };
    let i = Information { data: vec![
        0b_0000_0000,
        0b_0010_0000,
        0b_0000_0000
    ]};

    let int = t.apply_to(&i);

    assert_eq!(int.data, vec![
        0b_0001_0000,
        0b_0000_0000,
        0b_0000_0000
    ]);
}

#[test]
fn can_shift_vertical_down_and_h_left() {
    let t = Transformation { x: -1, y: -1, a: 0.0 };
    let i = Information { data: Vec::<i8>::from([
        0b_0000_0000,
        0b_0010_0000,
        0b_0000_0000
    ])};

    let int = t.apply_to(&i);

    assert_eq!(int.data, Vec::<i8>::from([
        0b_0000_0000,
        0b_0000_0000,
        0b_0100_0000
    ]));
}

#[test]
fn can_rotate_frac_pi_6() {
    let a = FRAC_PI_6;
    let t = Transformation { x: 0, y: 0, a: a };
    let i = Information { data: Vec::<u16>::from([
        0b_0000000000000000, // 0
        0b_0000000000000000, // 1
        0b_0000000000000000, // 2
        0b_1111111111111111, // 3
        0b_1111111111111111, // 4
        0b_0000000000000000, // 5
        0b_0000000000000000, // 6
        0b_0000000000000000, // 7
    ])};

    let int = t.apply_to(&i);

    assert_eq!(int.data, vec![
        0b_0000000000001110, // 0
        0b_0000000000011110, // 1
        0b_0000000001111000, // 2
        0b_0000000111100000, // 3
        0b_0000011110000000, // 4
        0b_0000111100000000, // 5
        0b_0011110000000000, // 6
        0b_1111000000000000, // 7
    ]);
}

#[test]
fn can_rotate_frac_pi_3() {
    let a = FRAC_PI_3;
    let t = Transformation { x: 0, y: 0, a: a };
    let i = Information { data: Vec::<u16>::from([
        0b_0000000000000000, // 0
        0b_0000000000000000, // 1
        0b_0000000000000000, // 2
        0b_1111111111111111, // 3
        0b_1111111111111111, // 4
        0b_0000000000000000, // 5
        0b_0000000000000000, // 6
        0b_0000000000000000, // 7
    ])};

    let int = t.apply_to(&i);

    assert_eq!(int.data, vec![
        0b_0000000001100000, // 0
        0b_0000000011100000, // 1
        0b_0000000011000000, // 2
        0b_0000000111000000, // 3
        0b_0000000110000000, // 4
        0b_0000001100000000, // 5
        0b_0000001100000000, // 6
        0b_0000011000000000, // 7
    ]);
}

#[test]
fn can_rotate_frac_pi_2() {
    let a = FRAC_PI_2;
    let t = Transformation { x: 0, y: 0, a: a };
    let i = Information { data: Vec::<u16>::from([
        0b_0000000000000000, // 0
        0b_0000000000000000, // 1
        0b_0000000000000000, // 2
        0b_1111111111111111, // 3
        0b_1111111111111111, // 4
        0b_0000000000000000, // 5
        0b_0000000000000000, // 6
        0b_0000000000000000, // 7
    ])};

    let int = t.apply_to(&i);

    assert_eq!(int.data, vec![
        0b_0000000110000000, // 0
        0b_0000000110000000, // 1
        0b_0000000110000000, // 2
        0b_0000000110000000, // 3
        0b_0000000110000000, // 4
        0b_0000000110000000, // 5
        0b_0000000110000000, // 6
        0b_0000000110000000, // 7
    ]);
}

#[test]
fn can_rotate_frac_2pi_3() {
    let a = 2.0 * FRAC_PI_3;
    let t = Transformation { x: 0, y: 0, a: a };
    let i = Information { data: Vec::<u16>::from([
        0b_0000000000000000, // 0
        0b_0000000000000000, // 1
        0b_0000000000000000, // 2
        0b_1111111111111111, // 3
        0b_1111111111111111, // 4
        0b_0000000000000000, // 5
        0b_0000000000000000, // 6
        0b_0000000000000000, // 7
    ])};

    let int = t.apply_to(&i);

    assert_eq!(int.data, vec![
        0b_0000111000000000, // 0
        0b_0000011000000000, // 1
        0b_0000001100000000, // 2
        0b_0000001110000000, // 3
        0b_0000000110000000, // 4
        0b_0000000111000000, // 5
        0b_0000000011000000, // 6
        0b_0000000011100000, // 7
    ]);
}

#[test]
fn can_rotate_frac_5pi_6() {
    let a = 5.0 * FRAC_PI_6;
    let t = Transformation { x: 0, y: 0, a: a };
    let i = Information { data: Vec::<u16>::from([
        0b_0000000000000000, // 0
        0b_0000000000000000, // 1
        0b_0000000000000000, // 2
        0b_1111111111111111, // 3
        0b_1111111111111111, // 4
        0b_0000000000000000, // 5
        0b_0000000000000000, // 6
        0b_0000000000000000, // 7
    ])};

    let int = t.apply_to(&i);

    assert_eq!(int.data, vec![
        0b_0010000000000000, // 0
        0b_0011000000000000, // 1
        0b_0011110000000000, // 2
        0b_0000111100000000, // 3
        0b_0000001110000000, // 4
        0b_0000000111100000, // 5
        0b_0000000001111000, // 6
        0b_0000000000011110, // 7
    ]);
}

#[test]
fn can_rotate_2pi() {
    let a = 2.0 * PI;
    let t = Transformation { x: 0, y: 0, a: a };
    let i = Information { data: Vec::<u16>::from([
        0b_0000000000000000, // 0
        0b_0000000000000000, // 1
        0b_0000000000000000, // 2
        0b_1111111111111111, // 3
        0b_1111111111111111, // 4
        0b_0000000000000000, // 5
        0b_0000000000000000, // 6
        0b_0000000000000000, // 7
    ])};

    let int = t.apply_to(&i);

    assert_eq!(int.data, vec![
        0b_0000000000000000, // 0
        0b_0000000000000000, // 1
        0b_0000000000000000, // 2
        0b_1111111111111111, // 3
        0b_1111111111111111, // 4
        0b_0000000000000000, // 5
        0b_0000000000000000, // 6
        0b_0000000000000000, // 7
    ]);
}