#[macro_use]
extern crate derive_more;
mod diffspace;
use diffspace::bitvector::BitVector;
use diffspace::code_space::CodeSpace;
use diffspace::context_map::ContextMap;
use diffspace::learn;
use rand::Rng;

fn main() {
	let codes = diffspace::code_space::generate_code_space();
	// draw_codes(&codes);

	const CONTEXT_MAP_MAX_DIM: usize = 64;
	let mut contexts = diffspace::context_map::generate_context_map(CONTEXT_MAP_MAX_DIM);

	let mut inputs: Vec<_> = codes.iter().cloned().collect();

	let mut rng = rand::thread_rng();

	let mut random_code_idx = 0;
	while random_code_idx == 0 || random_code_idx == inputs.len() - 1 {
		random_code_idx = rng.gen_range(0, inputs.len());
	}

	let test_code_r = inputs[random_code_idx];
	let test_code_g = inputs[random_code_idx + 1];
	let test_code_b = inputs[random_code_idx - 1];

	for step in 1.. {
		rng.shuffle(&mut inputs);

		for code in inputs.iter() {
			learn(&mut contexts, &code);
		}

		// print every n-th step
		if step % 10000 > 0 {
			let file_name = format!("r_{}.png", step);
			draw_contexts_activation_map(&contexts, &test_code_r, &file_name);
			let file_name = format!("g_{}.png", step);
			draw_contexts_activation_map(&contexts, &test_code_g, &file_name);
			let file_name = format!("b_{}.png", step);
			draw_contexts_activation_map(&contexts, &test_code_b, &file_name);
		}
	}
}

fn draw_codes(codes: &CodeSpace) {
	let width = 128;
	let height = codes.len() as u32;
	let mut image = image::GrayImage::new(width, height);

	let mut row = 0;
	for (code) in codes.iter() {
		for i in 0..width {
			let brightness = if code[i as usize] { 255 } else { 0 };

			let pixel: image::Luma<u8> = image::Luma([brightness]);
			image.put_pixel(i as u32, row as u32, pixel);
		}
		row += 1;
	}

	image.save("output/code_space.png").unwrap();
}

fn draw_contexts_activation_map(contexts: &ContextMap, code: &BitVector, file_name: &String) {
	let shape = contexts.shape();
	let height = (shape[0] + 2) as u32; //2 is for printing the code on the top
	let width = 128;

	//type GrayImage = ImageBuffer<Luma<u8>, Vec<u8>>;
	let mut image = image::GrayImage::new(width, height);

	// 1. Print contexts activation map
	for ((y, x), context) in contexts.indexed_iter() {
		let covariance_round = 5. * context.covariance(code).round();

		let brightness: u8 = if covariance_round > 255. {
			255
		} else {
			covariance_round as u8
		};

		let pixel: image::Luma<u8> = image::Luma([brightness]);
		image.put_pixel(x as u32, y as u32, pixel);
	}

	// 2. Print separator
	let pixel: image::Luma<u8> = image::Luma([128]);
	let y = height - 2;
	for x in 0..width {
		image.put_pixel(x, y, pixel);
	}

	// 3. Print the code: white is 1
	let y = height - 1;
	for x in 0..128 {
		let brightness = if code[x as usize] { 255 } else { 0 };

		let pixel: image::Luma<u8> = image::Luma([brightness]);
		image.put_pixel(x, y, pixel);
	}

	image
		.save(format!("output/activation_maps/{}", file_name))
		.unwrap();
}
