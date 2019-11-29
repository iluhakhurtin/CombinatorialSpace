pub mod analytics;
pub mod bitvector;
pub mod code_space;
mod context;
pub mod context_map;
use bitvector::BitVector;
use context_map::ContextMap;
use ndarray::Array2;

pub fn learn(contexts: &mut ContextMap, code: &BitVector) {
	//get covariances for the contexts
	let covariances = calculate_covariances_map(&contexts, &code);

	// for ((x, y), cov) in covariances.indexed_iter() {
	// 	println!("x: {0}, y: {1}, cov: {2}", x, y, cov);
	// }
}

fn calculate_covariances_map(contexts: &ContextMap, code: &BitVector) -> Array2<f32> {
	let covariances: Array2<f32> = contexts.map(|context| {
		let covariance = context
			.memory
			.iter()
			.map(|(memory_code, hits)| {
				let correlation = analytics::correlation(&memory_code.value(), &code.value());
				let full_correlation = *hits as f32 * correlation;
				full_correlation
			})
			.sum();

		covariance
	});
	covariances
}

fn build_contexts_distances(covariances: Array2<f32>) -> (f32, Vec<(usize, usize)>) {
	(0., vec![])
}
