use super::analytics;
use super::bitvector::BitVector;
use super::context_memory_item::ContextMemoryItem;

#[derive(Default, Debug, Clone)]
pub struct Context {
	pub memory: Vec<ContextMemoryItem>,
}

impl Context {
	pub fn covariance(&self, &code: &BitVector) -> f32 {
		let covariance = self
			.memory
			.iter()
			.map(|memory_item| {
				let correlation = analytics::correlation(&memory_item.code.value(), &code.value());
				// use an activation function here, for example, ReLu=max(const, x)
				// result of the function can be multiplied on hits to get 0 if the correlation
				// is not enough and essential increase /if there is one
				const threshold_correlation: f32 = 0.7;
				let correlation = if correlation > threshold_correlation {
					correlation
				} else {
					0f32
				};

				let result = memory_item.hits as f32 * correlation;
				result
			})
			.sum();

		if covariance == 0f32 {
			let result = Context::get_min_covariance();
			return result;
		}
		covariance
	}

	fn get_min_covariance() -> f32 {
		0.2
	}

	fn get_min_hits_to_retain() -> u32 {
		2
	}

	pub fn consolidate(&mut self) {
		let min_hits_to_retain = Context::get_min_hits_to_retain();

		self.memory.retain(|memory_item| {
			let keep = memory_item.hits > min_hits_to_retain;
			keep
		});
	}
}
