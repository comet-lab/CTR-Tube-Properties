% Data analysis for the experiment performed on 3/19/2024
close all; clear; clc;

% Times
t = [0 1 2 3 4 5 10 15 20 25 30];

% Number of speciments per tube OD
n = 5;

% Tube dimensions
OD = [2.64];

% Outer tube
OD_outer = 0.00264; % [m]
ID_outer = 0.002132; % [m]
k0_outer_0 = 1000 ./ [116.34571	111.79054	103.93466	111.18182	110.10815]; % [m^-1]
k0_outer_f = 1000 ./ [132.56567	124.58774	119.51005	128.29112	121.31294]; % [m^-1]

% Inner tube
OD_inner = 0.001980; % [m]
ID_inner = 0.001473; % [m] 
k0_inner = zeros([1 n]); % [m^-1]

% Moments of area
I_outer = pi/64 * (OD_outer ^ 4 - ID_outer ^ 4);
I_inner = pi/64 * (OD_inner ^ 4 - ID_inner ^ 4);

% Calc equilibrium curvature
k_eq_0 = (I_outer * k0_outer_0 + I_inner * k0_inner) / (I_outer + I_inner);
k_eq_f = (I_outer * k0_outer_f + I_inner * k0_inner) / (I_outer + I_inner);

% Calculate the Bending Radius: Nominal and at t+0hr, t+0.5hr, and t+4hr (mm)
r_expected_0 = 1./k_eq_0 * 1000;
r_expected_f = 1./k_eq_f * 1000;
r0 = [163.96829	152.60276	152.717	158.86293	151.43352];
r1 = [167.17217	158.14961	156.54795	164.59333	152.29993];
r2 = [171.12783	157.03563	158.00595	164.8651	153.03895];
r3 = [172.8245	158.60564	158.4371667	166.97496	152.45823];
r4 = [172.26807	160.08575	158.22185	167.71882	154.89371];
r5 = [172.98626	162.0468	160.93645	169.45387	152.59967];
r10 = [173.2665	161.89352	160.69986	170.34464	155.35012];
r15 = [176.89257	161.1974	160.89973	171.89173	155.45192];
r20 = [178.47626	161.64841	162.01341	171.51401	154.74017];
r25 = [178.77399	162.58633	162.20889	173.0765875	155.33082];
r30 = [179.00462	163.5390222	163.3554222	173.69946	156.4227125];

%% Formatting

titleSize = 24;
axisLabelSize = 20;
legendSize = 20;
tickSize = 16;
lineWidth = 2.5;
legendLocs = ["south", "northeast", "northeast", "south", "northeast"];

%% Plot curvature loss by batch

% % Plot the loss of curvature over time
% for jj = 1 : n
%     figure('WindowState','maximized'); grid on; hold on;
%     title(["Progressive Loss of Curvature for Tubes in batch #" num2str(jj)], "FontSize",titleSize);
% 
%     for ii = 1 : length(OD)
%         radii = [r0(jj+n*(ii-1)) r0_5(jj+n*(ii-1)) r1(jj+n*(ii-1)) r4(jj+n*(ii-1)) r24(jj+n*(ii-1)) r48(jj+n*(ii-1))];
%         plot(t(isfinite(radii)), radii(isfinite(radii)),"-o","LineWidth",lineWidth);
%         xlabel("Time Since Heat-Set [Hours]", "FontSize", axisLabelSize);
%         ylabel("Bending Radius [mm]", "FontSize", axisLabelSize);
%         ylim([45 140]);
%     end
% 
%     plot([0 t(end)], [r_expected, r_expected], "LineWidth", 2.5, "Color", "red");
% 
%     legend({"Thickest (OD=3.3 mm)", "Thicker (OD=2.64 mm)", "Thinner (OD=1.98 mm)", "Nominal"}, "Location", "northwest");
% 
%     saveas(gcf,"./relaxation/batch"+num2str(jj)+".png");
% end

%% Plot the loss of curvature over time / group by batch

for jj = 1 : n
    figure('WindowState','maximized'); grid on; hold on;
    
    radii = [r0(jj) r1(jj) r2(jj) r3(jj) r4(jj) r5(jj) r10(jj) r15(jj) r20(jj) r25(jj) r30(jj)];
    plot(t(isfinite(radii)), radii(isfinite(radii)),"-o","LineWidth",lineWidth);
    
    plot([0 t(end)], [r_expected_0(jj), r_expected_0(jj)], "LineWidth", 2.5, "Color", "red");
    plot([0 t(end)], [r_expected_f(jj), r_expected_f(jj)], "LineWidth", 2.5, "Color", "green");

    ax=gca;
    ax.FontSize = tickSize;

    xlabel("Time Since Tube Insertion [Minutes]", "FontSize", axisLabelSize);
    ylabel("Equilibrium Bending Radius [mm]", "FontSize", axisLabelSize);
    ylim([140 185]);

    title_string = "Equilibrium Curvature of Nested Tubes in Trial " + num2str(jj) + " Over Time";
    title(title_string, "FontSize",titleSize);

    legend({"Observed", "Expected (Using Initial Curvature)", "Expected (Using Deformed Curvature)"}, "Location", legendLocs(jj), "FontSize", legendSize);

    saveas(gcf,"./equilibrium_curvature/equilibrium"+num2str(jj)+".png");
end