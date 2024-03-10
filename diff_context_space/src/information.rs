extern crate num_traits;
use num_traits::int::PrimInt;

use std::fmt;

extern crate serde;
use serde::{Serialize, Deserialize};

extern crate image;
use image::{DynamicImage, GrayImage, Luma};


#[derive(Clone, Serialize, Deserialize)]
pub struct Information<T:PrimInt + Serialize> {
    pub data: Vec<T>,
    pub name: String
}

impl<T:PrimInt + Serialize> Information<T> {
    pub fn coherence_to(&self, to: &Information<T>) -> Result<f32, &str> {
        if self.data.len() != to.data.len() {
            return Err("Lengths do not match");
        }
        
        let mut to_ones_count = 0;
        let mut conj_ones_count = 0;
        for i in 0 .. self.data.len() {
            let self_byte = self.data[i];
            let to_byte = to.data[i];

            to_ones_count += to_byte.count_ones();

            let conj = self_byte & to_byte;
            conj_ones_count += conj.count_ones();
        }

        if to_ones_count == 0 {
            return Ok(0.);
        }

        let coherence = conj_ones_count as f32 / to_ones_count as f32;

        Ok(coherence)
    }

    pub fn get_empty_elt(&self) -> T {
        T::zero()
    }

    /// Creates an information from a grey image.
    /// White pixels are transformed as 1, black as 0.
    pub fn from_image(dyn_img: &DynamicImage, name: String) -> Information<T>
        where T: PrimInt {
        const THRESHOLD:u8 = 50;
        let i = Information::<T>::from_image_threshold(dyn_img, THRESHOLD, name);
        i
    }

    /// This function creates an Information structure from every pixel
    /// of a grey image. The value of a pixel higher than the given threshold 
    /// is parsed as 1, lower as 0.
    pub fn from_image_threshold(dyn_img: &DynamicImage, threshold: u8, name: String) -> Information<T> {
        let img = dyn_img.to_luma8(); // Grey image
        let h = img.height();
        let mut data = Vec::<T>::with_capacity(h as usize);
        let w = img.width();

        for row in 0..h {
            let mut mask = T::one();
            let mut d = T::zero();

            for col in 0..w {
                // X coordinate of a pixel starts from left,
                // but data starts from the right.
                // As a result, a column value is opposite between them.
                // That is why x pixel position is calculated as (w - 1 - col).
                let p = img.get_pixel(w - 1 - col, row);
                let p_val = p.0[0];
                if p_val > threshold {
                    d = d | mask;
                }
                mask = mask.unsigned_shl(1);
            }

            data.push(d);
        }

        Information { data, name }
    }

    pub fn to_image(&self) -> DynamicImage {
        let zero = T::zero();
        let w = zero.count_zeros();
        let h = self.data.len() as u32;
        let mut img = GrayImage::new(w, h);
        for row in 0..h {
            let mut mask = T::one();
            for col in 0..w {
                let pixel = if (self.data[row as usize] & mask) > zero {
                    Luma([255])
                } else {
                    Luma([0])
                };

                // X coordinate of a pixel starts from left,
                // but data starts from the right.
                // As a result, a column value is opposite between them.
                // That is why x pixel position is calculated as (w - 1 - col).
                img.put_pixel(w - 1 - col, row, pixel);

                mask = mask.unsigned_shl(1);
            }
        }
        DynamicImage::ImageLuma8(img)
    }
}

impl<T: PrimInt + Serialize> fmt::Display for Information<T> where T: fmt::Binary + PrimInt {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        for d in &self.data {
            let res = writeln!(f, "{:#010b}", d);
            match res {
                Ok(_) => (),
                Err(e) => return Err(e),
            };
        }
        return Ok(());
    }
}

impl<T: PrimInt + Serialize> std::cmp::PartialEq for Information<T> where T: PrimInt {
    fn eq(&self, other: &Self) -> bool {
        if self.name != other.name {
            return false;
        }

        for i in 0..self.data.len() {
            if self.data[i] != other.data[i] {
                return false;
            }
        }
        return true;
    }
}

#[test]
fn can_calculate_coherence_to() {
    let d = vec![0x01, 0x01, 0x01, 0x01, 0x01];
    let n = String::from("name1");
    let i1 = Information { data: d, name: n};

    let d = vec![0x01, 0x02, 0x01, 0x01, 0x01];
    let n = String::from("name2");
    let i2 = Information { data: d, name: n};

    let c = i1.coherence_to(&i2);
    assert_eq!(c, Ok(0.8));
}

#[test]
fn can_calculate_full_coherence_to() {
    let d = vec![0x01, 0x01, 0x01, 0x01, 0x01];
    let n = String::from("name1");
    let i1 = Information { data: d, name: n };

    let d = vec![0x01, 0x01, 0x01, 0x01, 0x01];
    let n = String::from("name2");
    let i2 = Information { data: d, name: n };

    let c = i1.coherence_to(&i2);
    assert_eq!(c, Ok(1.0));
}

#[test]
fn can_get_error_on_coherence_to_calculation() {
    let d = vec![0x01, 0x01, 0x01, 0x01, 0x01];
    let n = String::from("name1");
    let i1 = Information { data: d, name: n };

    let d = vec![0x01, 0x02, 0x01, 0x01];
    let n = String::from("name2");
    let i2 = Information { data: d, name: n };

    let e = i1.coherence_to(&i2);
    assert_eq!(e, Err("Lengths do not match"));
}

#[test]
fn can_build_from_image() {
    let mut path = std::path::PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    path.push("files/tests/information/smile_bw_16x16.png");
    
    let name = String::from("smile_bw_16x16.png");
    let img = image::open(path).unwrap();
    
    let actual = Information::<u16>::from_image(&img, name);

    // The data pieces are shown in a reflected to the right
    // way because a number here has lower index in the right
    // but higher in the left. This is just representation of 
    // the numbers. 
    let data = vec!(
        0b_0111111111111110,    // 0
        0b_1000000000000001,    // 1
        0b_1000000000000001,    // 2
        0b_1000100000010001,    // 3
        0b_1001010000101001,    // 4
        0b_1000000000000001,    // 5
        0b_1000000000000001,    // 6
        0b_1000000000000001,    // 7
        0b_1011111111111101,    // 8
        0b_1010101010101101,    // 9
        0b_1001101010101001,    // 10
        0b_1001101010111001,    // 11
        0b_1000101010110001,    // 12
        0b_1000011111100001,    // 13
        0b_1000000000000001,    // 14
        0b_0111111111111110,    // 15
    );
    let expected = Information { data, name };

    assert_eq!(actual.data[0], expected.data[0]);
    assert_eq!(actual.data[1], expected.data[1]);
    assert_eq!(actual.data[2], expected.data[2]);
    assert_eq!(actual.data[3], expected.data[3]);
    assert_eq!(actual.data[4], expected.data[4]);
    assert_eq!(actual.data[5], expected.data[5]);
    assert_eq!(actual.data[6], expected.data[6]);
    assert_eq!(actual.data[7], expected.data[7]);
    assert_eq!(actual.data[8], expected.data[8]);
    assert_eq!(actual.data[9], expected.data[9]);
    assert_eq!(actual.data[10], expected.data[10]);
    assert_eq!(actual.data[11], expected.data[11]);
    assert_eq!(actual.data[12], expected.data[12]);
    assert_eq!(actual.data[13], expected.data[13]);
    assert_eq!(actual.data[14], expected.data[14]);
    assert_eq!(actual.data[15], expected.data[15]);
}

#[test]
fn can_to_image() {
    let data = vec!(
        0b_0111111111111110,    // 0
        0b_1000000000000001,    // 1
        0b_1000000000000001,    // 2
        0b_1000100000010001,    // 3
        0b_1001010000101001,    // 4
        0b_1000000000000001,    // 5
        0b_1000000000000001,    // 6
        0b_1000000000000001,    // 7
        0b_1011111111111101,    // 8
        0b_1011010101010101,    // 9
        0b_1001010101011001,    // 10
        0b_1001110101011001,    // 11
        0b_1000110101010001,    // 12
        0b_1000011111100001,    // 13
        0b_1000000000000001,    // 14
        0b_0111111111111110,    // 15
    );
    let name = String::from("name1");
    let expected = Information::<u16> { data, name };

    let img = expected.to_image();
    let actual = Information::<u16>::from_image(&img, name);

    assert_eq!(actual.data[0], expected.data[0]);
    assert_eq!(actual.data[1], expected.data[1]);
    assert_eq!(actual.data[2], expected.data[2]);
    assert_eq!(actual.data[3], expected.data[3]);
    assert_eq!(actual.data[4], expected.data[4]);
    assert_eq!(actual.data[5], expected.data[5]);
    assert_eq!(actual.data[6], expected.data[6]);
    assert_eq!(actual.data[7], expected.data[7]);
    assert_eq!(actual.data[8], expected.data[8]);
    assert_eq!(actual.data[9], expected.data[9]);
    assert_eq!(actual.data[10], expected.data[10]);
    assert_eq!(actual.data[11], expected.data[11]);
    assert_eq!(actual.data[12], expected.data[12]);
    assert_eq!(actual.data[13], expected.data[13]);
    assert_eq!(actual.data[14], expected.data[14]);
    assert_eq!(actual.data[15], expected.data[15]);
}

#[test]
fn can_read_same_image() {
    let mut expected_path = std::path::PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    expected_path.push("files/tests/information/smile_bw_16x16.png");
    let expected_img = image::open(expected_path).unwrap();
    let name = String::from("smile_bw_16x16.png");

    let expected = Information::<u16>::from_image(&expected_img, name);
    let img = expected.to_image();
    
    let mut path = std::path::PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    path.push("files/tests/information/smile_out_16x16.png");
    img.save_with_format(&path, image::ImageFormat::Png).unwrap();

    let actual_img = image::open(&path).unwrap();
    let actual = Information::<u16>::from_image(&actual_img, name);
    
    assert_eq!(actual.data[0], expected.data[0]);
    assert_eq!(actual.data[1], expected.data[1]);
    assert_eq!(actual.data[2], expected.data[2]);
    assert_eq!(actual.data[3], expected.data[3]);
    assert_eq!(actual.data[4], expected.data[4]);
    assert_eq!(actual.data[5], expected.data[5]);
    assert_eq!(actual.data[6], expected.data[6]);
    assert_eq!(actual.data[7], expected.data[7]);
    assert_eq!(actual.data[8], expected.data[8]);
    assert_eq!(actual.data[9], expected.data[9]);
    assert_eq!(actual.data[10], expected.data[10]);
    assert_eq!(actual.data[11], expected.data[11]);
    assert_eq!(actual.data[12], expected.data[12]);
    assert_eq!(actual.data[13], expected.data[13]);
    assert_eq!(actual.data[14], expected.data[14]);
    assert_eq!(actual.data[15], expected.data[15]);
}

#[test]
fn can_equal() {
    let mut path = std::path::PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    path.push("files/tests/information/smile_bw_16x16.png");
    let img = image::open(path).unwrap();
    let name = String::from("smile_bw_16x16.png");

    let i1 = Information::<u16>::from_image(&img, name);

    // The data pieces are shown in a reflected to the right
    // way because a number here has lower index in the right
    // but higher in the left. This is just representation of 
    // the numbers. 
    let data = vec!(
        0b_0111111111111110,    // 0
        0b_1000000000000001,    // 1
        0b_1000000000000001,    // 2
        0b_1000100000010001,    // 3
        0b_1001010000101001,    // 4
        0b_1000000000000001,    // 5
        0b_1000000000000001,    // 6
        0b_1000000000000001,    // 7
        0b_1011111111111101,    // 8
        0b_1010101010101101,    // 9
        0b_1001101010101001,    // 10
        0b_1001101010111001,    // 11
        0b_1000101010110001,    // 12
        0b_1000011111100001,    // 13
        0b_1000000000000001,    // 14
        0b_0111111111111110,    // 15
    );
    let i2 = Information { data, name };

    let actual = i1 == i2;

    assert_eq!(actual, true);
}

#[test]
fn can_not_equal() {
    let mut path = std::path::PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    path.push("files/tests/information/smile_bw_16x16.png");
    let img = image::open(path).unwrap();
    let name = String::from("smile_bw_16x16.png");

    let i1 = Information::<u16>::from_image(&img, name);

    // The data pieces are shown in a reflected to the right
    // way because a number here has lower index in the right
    // but higher in the left. This is just representation of 
    // the numbers. 
    let data = vec!(
        0b_0111111111111110,    // 0
        0b_1000000000000001,    // 1
        0b_1000000000000001,    // 2
        0b_1000100000010001,    // 3
        0b_1001010000101000,    // 4 exptected to have 0b_1001010000101001 to be equal
        0b_1000000000000001,    // 5
        0b_1000000000000001,    // 6
        0b_1000000000000001,    // 7
        0b_1011111111111101,    // 8
        0b_1011010101010101,    // 9
        0b_1001010101011001,    // 10
        0b_1001110101011001,    // 11
        0b_1000110101010001,    // 12
        0b_1000011111100001,    // 13
        0b_1000000000000001,    // 14
        0b_0111111111111110,    // 15
    );
    let i2 = Information { data, name };

    let actual = i1 != i2;

    assert_eq!(actual, true);
}