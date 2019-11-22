pub fn correlation(x: u128, y: u128) -> f32 {
	let nx = x.count_ones();
	let ny = y.count_ones();
	let s = (x & y).count_ones();
	let product = nx * ny;

	if product == 0 {
		0.
	} else {
		s as f32 / (product as f32).sqrt()
	}
}
