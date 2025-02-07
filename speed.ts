export function calculateGroundVehicleSpeed(model: VehicleModel): number {
  const data = model.VehiclePhys;

  return Math.round(
    ((data.engine.maxRPM * data.mechanics.driveGearRadius) /
      (data.mechanics.mainGearRatio *
        data.mechanics.sideGearRatio *
        data.mechanics.gearRatios[
          data.mechanics.gearRatios.ratio.length - 1
        ])) *
      0.12 *
      Math.PI,
  );
}