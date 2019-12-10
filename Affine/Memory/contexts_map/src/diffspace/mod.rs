pub mod analytics;
pub mod bitvector;
pub mod code_space;
mod context;
pub mod context_map;
mod context_memory_item;
use bitvector::BitVector;
use context::Context;
use context_map::ContextMap;
use context_memory_item::ContextMemoryItem;
use float_cmp::ApproxOrdUlps;
use ndarray::{s, Array2};
use num_complex::Complex32 as Complex;
use rand::Rng;

pub fn learn(contexts: &mut ContextMap, code: &BitVector) {
	// get covariances for the contexts
	let covariances = calculate_covariances_map(&contexts, &code);

	// get contexts' covariances distances map with total distance
	let (total_distance, distances) = calculate_contexts_covariances_distances(&covariances);

	// winner context's coordinates
	let candidate_coordinates = get_winner_coordinates(&total_distance, &distances);
	let candidate_center = Complex::new(
		candidate_coordinates.1 as f32,
		candidate_coordinates.0 as f32,
	);

	// get contexts map dimentions
	let learn_range = get_learn_range();

	let shape = contexts.shape();
	let max_dim_y = shape[0];
	let start_y = candidate_coordinates.0 as isize - learn_range;
	let end_y = candidate_coordinates.0 as isize + learn_range;
	let range_y = clamp_index(&start_y, &max_dim_y)..=clamp_index(&end_y, &max_dim_y);

	let max_dim_x = shape[1];
	let start_x = candidate_coordinates.1 as isize - learn_range;
	let end_x = candidate_coordinates.1 as isize + learn_range;
	let range_x = clamp_index(&start_x, &max_dim_x)..=clamp_index(&end_x, &max_dim_x);

	let mut learn_region = contexts.slice_mut(s![range_y.clone(), range_x.clone()]);

	let max_learn_distance = get_max_learn_distance();
	for ((y, x), context) in learn_region.indexed_iter_mut() {
		// Calculate distance from the target context to current one from its immediate surroundings.
		let (real_y, real_x) = (y + range_y.start(), x + range_x.start());
		let distance = (Complex::new(real_x as f32, real_y as f32) - candidate_center).norm();

		if distance > max_learn_distance {
			continue;
		}

		// update_context(context, code);
	}
}

fn calculate_covariances_map(contexts: &ContextMap, code: &BitVector) -> Array2<f32> {
	let covariances: Array2<f32> = contexts.map(|context| {
		let covariance = context
			.memory
			.iter()
			.map(|memory_item| {
				let correlation = analytics::correlation(&memory_item.code.value(), &code.value());
				let full_correlation = memory_item.hits as f32 * correlation;
				full_correlation
			})
			.sum();

		if covariance == 0f32 {
			let result = get_min_covariance();
			return result;
		}
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
fn calculate_contexts_covariances_distances(
	covariances: &Array2<f32>,
) -> (f32, Vec<((usize, usize), f32)>) {
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

fn get_winner_coordinates(
	total_distance: &f32,
	distances: &Vec<((usize, usize), f32)>,
) -> (usize, usize) {
	let mut rng = rand::thread_rng();

	// Random value to be used for candidate selection.
	// The larger the covariance, the more chances are that
	// this context will be selected by the binary search.
	let pick = rng.gen_range(0., total_distance);

	// Find an index of a segment that encloses random value.
	let target_index =
		match distances.binary_search_by(|(_, distance)| distance.approx_cmp_ulps(&pick, 2)) {
			Ok(index) => index,
			Err(index) => index,
		};

	let target_coordinates = distances[target_index].0;

	target_coordinates
}

fn clamp_index(index: &isize, max_dim: &usize) -> usize {
	(match index {
		_ if *index < 0 => 0,
		_ if *index > (max_dim - 1) as isize => max_dim - 1,
		_ => *index as usize,
	})
}

fn update_context(context: &mut Context, code: BitVector) {
	// Find a memory item with the lower hits value in the same
	// loop for the case if a match is not found
	let mut min_hits_value: i32 = -1;
	let mut min_hits_idx: usize = 0;
	for (idx, memory_item) in context.memory.iter_mut().enumerate() {
		if memory_item.code.value() == code.value() {
			memory_item.hits += 1;
			return;
		}

		if min_hits_value == -1 {
			min_hits_value = memory_item.hits as i32;
			min_hits_idx = idx;
			continue;
		}

		if min_hits_value > (memory_item.hits as i32) {
			min_hits_value = memory_item.hits as i32;
			min_hits_idx = idx;
			continue;
		}
	}

	// add new element in the memory and if the memory is full
	// remove another with the minimum hits
	let max_memory_size = get_max_memory_size();
	if context.memory.len() == max_memory_size {
		context.memory.remove(min_hits_idx);
	}

	let new_memory_item = ContextMemoryItem {
		code: code.clone(),
		hits: 0,
	};
	context.memory.push(new_memory_item);
}

fn get_min_covariance() -> f32 {
	0.0001
}

fn get_learn_range() -> isize {
	4
}

fn get_max_learn_distance() -> f32 {
	5.
}

fn get_max_memory_size() -> usize {
	20
}
