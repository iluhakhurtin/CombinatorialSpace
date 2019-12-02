pub mod analytics;
pub mod bitvector;
pub mod code_space;
mod context;
pub mod context_map;
use bitvector::BitVector;
use context_map::ContextMap;
use ndarray::Array2;
use float_cmp::ApproxOrdUlps;

pub fn learn(contexts: &mut ContextMap, code: &BitVector) {
	// get covariances for the contexts
	let covariances = calculate_covariances_map(&contexts, &code);

    // get contexts' covariances distances map with total distance
    let (total_distance, distances) = calculate_contexts_covariances_distances(&covariances);

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

/// Builds condexts distances map in which every context's coordinates
/// are placed alongside a line in which the distances are represented
/// as the gaps between numbers. The more covariance is the bigger gap
/// the contexts occupies on the line. 
/// The idea is to increase the chnances to be randomly selected for a 
/// context which has higher covariance:
/// 1 1 1 10 100 10 1 1 1 1
/// 1 2 3 13 113 123 124 125.
/// The method returns total distance (the length of the line) and a 
/// vector which links coordinates of a context and the distance.
fn calculate_contexts_covariances_distances(covariances: &Array2<f32>) -> (f32, Vec<((usize, usize), f32)>) {
    // Total distance to be used in weighted random selection.
    let mut total_distance: f32 = 0.;

    // Selection array. Each candidate is placed along the imaginary line,
    // occupying a line segment propoptional to context's covariance.
    let contexts_distances: Vec<_> = covariances
        .indexed_iter()
        .map(|(coords, covariance)| {
            total_distance += covariance;
            (coords, total_distance)
        })
        .collect();

	(total_distance, contexts_distances)
}

fn get_candidate_coordinates(distances: &Vec<((usize, usize), f32)>, total_distance: &f32) -> (usize, usize) {
    let mut rng = rand::thread_rng();

    // Random value to be used for candidate selection.
    // The larger the covariance, the more chances are that
    // this context will be selected by the binary search.
    let pick = rng.gen_range(0., total_distance);

    // Find an index of a segment that encloses random value.
    let target_index = match distances.binary_search_by(|(_, distance)| distance.approx_cmp_ulps(&pick, 2)) {
        Ok(index) => index,
        Err(index) => index,
    };



    (1, 1)
}