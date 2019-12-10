#[macro_use]
extern crate derive_more;
mod diffspace;
use diffspace::bitvector::BitVector;
use diffspace::context_map::ContextMap;
use diffspace::learn;
use rand::Rng;

fn main() {
	let codes = diffspace::code_space::generate_code_space();
	const CONTEXT_MAP_MAX_DIM: usize = 64;
	let mut contexts = diffspace::context_map::generate_context_map(CONTEXT_MAP_MAX_DIM);

	let mut rng = rand::thread_rng();
	let mut inputs: Vec<_> = codes.iter().cloned().collect();

	for step in 1.. {
		rng.shuffle(&mut inputs);

		for code in inputs.iter() {
			learn(&mut contexts, &code);
		}

		// print every 100-th step
		if step % 100 > 0 {

			// draw_contexts(&contexts, &code, &step);
		}
	}
}

fn draw_contexts(contexts: &ContextMap, code: &BitVector, step: &usize) {
	let shape = contexts.shape();
	let height = shape[0] as u32;
	let width = shape[1] as u32;

	let mut image = image::GrayImage::new(width, height);

	//let pixel: [u8, 3] =
}
